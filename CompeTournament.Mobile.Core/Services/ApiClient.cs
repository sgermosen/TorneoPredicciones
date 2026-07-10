namespace CompeTournament.Mobile.Core.Services
{
    using CompeTournament.Shared.Auth;
    using CompeTournament.Shared.Tournaments;
    using System.Net;
    using System.Net.Http.Headers;
    using System.Net.Http.Json;

    public class ApiClient : IApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly ITokenStore _tokenStore;

        public ApiClient(HttpClient httpClient, ITokenStore tokenStore)
        {
            _httpClient = httpClient;
            _tokenStore = tokenStore;
        }

        public async Task<TokenResponse> LoginAsync(TokenRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/token", request);
            await EnsureSuccessAsync(response);
            return await ReadAsync<TokenResponse>(response);
        }

        public async Task<TokenResponse> RefreshAsync(string refreshToken)
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/refresh", new RefreshRequest { RefreshToken = refreshToken });
            await EnsureSuccessAsync(response);
            return await ReadAsync<TokenResponse>(response);
        }

        public async Task LogoutAsync(string refreshToken)
        {
            var request = await CreateAuthorizedRequestAsync(HttpMethod.Post, "api/auth/logout");
            request.Content = JsonContent.Create(new RefreshRequest { RefreshToken = refreshToken });
            await _httpClient.SendAsync(request);
        }

        public async Task<UserDto> RegisterAsync(RegisterRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/register", request);
            await EnsureSuccessAsync(response);
            return await ReadAsync<UserDto>(response);
        }

        public Task<UserDto> GetMeAsync() => SendAsync<UserDto>(() => new HttpRequestMessage(HttpMethod.Get, "api/auth/me"));

        public Task<List<GroupDto>> GetGroupsAsync() => SendAsync<List<GroupDto>>(() => new HttpRequestMessage(HttpMethod.Get, "api/groups"));

        public Task<List<GroupDto>> GetMyGroupsAsync() => SendAsync<List<GroupDto>>(() => new HttpRequestMessage(HttpMethod.Get, "api/groups/mine"));

        public Task<GroupDetailDto> GetGroupAsync(int id) => SendAsync<GroupDetailDto>(() => new HttpRequestMessage(HttpMethod.Get, $"api/groups/{id}"));

        public Task JoinGroupAsync(int id) => SendAsync(() => new HttpRequestMessage(HttpMethod.Post, $"api/groups/{id}/join"));

        public Task<GroupInviteDto> GetInviteAsync(int id) => SendAsync<GroupInviteDto>(() => new HttpRequestMessage(HttpMethod.Get, $"api/groups/{id}/invite"));

        public Task<GroupDto> JoinByCodeAsync(string code) =>
            SendAsync<GroupDto>(() => new HttpRequestMessage(HttpMethod.Post, "api/groups/join-by-code")
            {
                Content = JsonContent.Create(new JoinByCodeRequest { Code = code })
            });

        public Task<List<LeaderboardEntryDto>> GetLeaderboardAsync(int id) =>
            SendAsync<List<LeaderboardEntryDto>>(() => new HttpRequestMessage(HttpMethod.Get, $"api/groups/{id}/leaderboard"));

        public Task<RecapDto> GetRecapAsync(int id) =>
            SendAsync<RecapDto>(() => new HttpRequestMessage(HttpMethod.Get, $"api/groups/{id}/recap"));

        public Task<MatchDto> GetMatchAsync(int id) => SendAsync<MatchDto>(() => new HttpRequestMessage(HttpMethod.Get, $"api/matches/{id}"));

        public Task<PredictionDto> SavePredictionAsync(PredictionRequest request) =>
            SendAsync<PredictionDto>(() => new HttpRequestMessage(HttpMethod.Post, "api/predictions")
            {
                Content = JsonContent.Create(request)
            });

        public Task<List<PredictionDto>> GetMyPredictionsAsync(int? groupId = null)
        {
            var path = groupId.HasValue ? $"api/predictions/mine?groupId={groupId.Value}" : "api/predictions/mine";
            return SendAsync<List<PredictionDto>>(() => new HttpRequestMessage(HttpMethod.Get, path));
        }

        public Task<InsightsDto> GetInsightsAsync(int? groupId = null)
        {
            var path = groupId.HasValue ? $"api/insights/me?groupId={groupId.Value}" : "api/insights/me";
            return SendAsync<InsightsDto>(() => new HttpRequestMessage(HttpMethod.Get, path));
        }

        public Task<List<CommentDto>> GetCommentsAsync(int matchId) =>
            SendAsync<List<CommentDto>>(() => new HttpRequestMessage(HttpMethod.Get, $"api/matches/{matchId}/comments"));

        public Task<CommentDto> PostCommentAsync(int matchId, string body) =>
            SendAsync<CommentDto>(() => new HttpRequestMessage(HttpMethod.Post, $"api/matches/{matchId}/comments")
            {
                Content = JsonContent.Create(new CommentRequest { Body = body })
            });

        private async Task<T> SendAsync<T>(Func<HttpRequestMessage> requestFactory)
        {
            var response = await SendWithRefreshAsync(requestFactory);
            await EnsureSuccessAsync(response);
            return await ReadAsync<T>(response);
        }

        private async Task SendAsync(Func<HttpRequestMessage> requestFactory)
        {
            var response = await SendWithRefreshAsync(requestFactory);
            await EnsureSuccessAsync(response);
        }

        private async Task<HttpResponseMessage> SendWithRefreshAsync(Func<HttpRequestMessage> requestFactory)
        {
            var response = await _httpClient.SendAsync(await ApplyTokenAsync(requestFactory()));
            if (response.StatusCode != HttpStatusCode.Unauthorized)
            {
                return response;
            }

            var refreshToken = await _tokenStore.GetRefreshTokenAsync();
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                return response;
            }

            var refreshResponse = await _httpClient.PostAsJsonAsync("api/auth/refresh", new RefreshRequest { RefreshToken = refreshToken });
            if (!refreshResponse.IsSuccessStatusCode)
            {
                await _tokenStore.ClearAsync();
                return response;
            }

            var tokens = await ReadAsync<TokenResponse>(refreshResponse);
            await _tokenStore.SetTokensAsync(tokens.Token, tokens.RefreshToken);

            return await _httpClient.SendAsync(await ApplyTokenAsync(requestFactory()));
        }

        private async Task<HttpRequestMessage> ApplyTokenAsync(HttpRequestMessage request)
        {
            var token = await _tokenStore.GetTokenAsync();
            if (!string.IsNullOrWhiteSpace(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            return request;
        }

        private async Task<HttpRequestMessage> CreateAuthorizedRequestAsync(HttpMethod method, string path)
        {
            return await ApplyTokenAsync(new HttpRequestMessage(method, path));
        }

        private static async Task<T> ReadAsync<T>(HttpResponseMessage response)
        {
            var value = await response.Content.ReadFromJsonAsync<T>();
            if (value == null)
            {
                throw new ApiException(response.StatusCode, "Respuesta vacia del servidor.");
            }

            return value;
        }

        private static async Task EnsureSuccessAsync(HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
            {
                return;
            }

            var message = response.StatusCode switch
            {
                HttpStatusCode.Unauthorized => "Credenciales invalidas o sesion expirada.",
                HttpStatusCode.Forbidden => "No tienes permiso para esta accion.",
                HttpStatusCode.Conflict => "El recurso ya existe.",
                HttpStatusCode.TooManyRequests => "Demasiados intentos, espera un momento.",
                _ => $"Error del servidor ({(int)response.StatusCode})."
            };

            throw new ApiException(response.StatusCode, message);
        }
    }
}
