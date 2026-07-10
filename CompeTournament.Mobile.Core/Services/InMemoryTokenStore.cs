namespace CompeTournament.Mobile.Core.Services
{
    public class InMemoryTokenStore : ITokenStore
    {
        private string? _token;
        private string? _refreshToken;

        public Task<string?> GetTokenAsync() => Task.FromResult(_token);

        public Task<string?> GetRefreshTokenAsync() => Task.FromResult(_refreshToken);

        public Task SetTokensAsync(string accessToken, string refreshToken)
        {
            _token = accessToken;
            _refreshToken = refreshToken;
            return Task.CompletedTask;
        }

        public Task ClearAsync()
        {
            _token = null;
            _refreshToken = null;
            return Task.CompletedTask;
        }
    }
}
