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

        public async Task<UserDto> RegisterAsync(RegisterRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/register", request);
            await EnsureSuccessAsync(response);
            return await ReadAsync<UserDto>(response);
        }

        public Task<UserDto> GetMeAsync() => GetAsync<UserDto>("api/auth/me");

        public Task<List<GroupDto>> GetGroupsAsync() => GetAsync<List<GroupDto>>("api/groups");

        public Task<List<GroupDto>> GetMyGroupsAsync() => GetAsync<List<GroupDto>>("api/groups/mine");

        public Task<GroupDetailDto> GetGroupAsync(int id) => GetAsync<GroupDetailDto>($"api/groups/{id}");

        public async Task JoinGroupAsync(int id)
        {
            var request = await CreateAuthorizedRequestAsync(HttpMethod.Post, $"api/groups/{id}/join");
            var response = await _httpClient.SendAsync(request);
            await EnsureSuccessAsync(response);
        }

        public Task<List<LeaderboardEntryDto>> GetLeaderboardAsync(int id) =>
            GetAsync<List<LeaderboardEntryDto>>($"api/groups/{id}/leaderboard");

        public Task<MatchDto> GetMatchAsync(int id) => GetAsync<MatchDto>($"api/matches/{id}");

        public async Task<PredictionDto> SavePredictionAsync(PredictionRequest request)
        {
            var message = await CreateAuthorizedRequestAsync(HttpMethod.Post, "api/predictions");
            message.Content = JsonContent.Create(request);
            var response = await _httpClient.SendAsync(message);
            await EnsureSuccessAsync(response);
            return await ReadAsync<PredictionDto>(response);
        }

        public Task<List<PredictionDto>> GetMyPredictionsAsync(int? groupId = null)
        {
            var path = groupId.HasValue ? $"api/predictions/mine?groupId={groupId.Value}" : "api/predictions/mine";
            return GetAsync<List<PredictionDto>>(path);
        }

        private async Task<T> GetAsync<T>(string path)
        {
            var request = await CreateAuthorizedRequestAsync(HttpMethod.Get, path);
            var response = await _httpClient.SendAsync(request);
            await EnsureSuccessAsync(response);
            return await ReadAsync<T>(response);
        }

        private async Task<HttpRequestMessage> CreateAuthorizedRequestAsync(HttpMethod method, string path)
        {
            var request = new HttpRequestMessage(method, path);
            var token = await _tokenStore.GetTokenAsync();
            if (!string.IsNullOrWhiteSpace(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            return request;
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
