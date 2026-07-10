using CompeTournament.Mobile.Core.Services;

namespace CompeTournament.Mobile.Services
{
    public class SecureStorageTokenStore : ITokenStore
    {
        private const string TokenKey = "competournament_auth_token";
        private const string RefreshKey = "competournament_refresh_token";

        public Task<string?> GetTokenAsync() => SecureStorage.Default.GetAsync(TokenKey);

        public Task<string?> GetRefreshTokenAsync() => SecureStorage.Default.GetAsync(RefreshKey);

        public async Task SetTokensAsync(string accessToken, string refreshToken)
        {
            await SecureStorage.Default.SetAsync(TokenKey, accessToken);
            await SecureStorage.Default.SetAsync(RefreshKey, refreshToken);
        }

        public Task ClearAsync()
        {
            SecureStorage.Default.Remove(TokenKey);
            SecureStorage.Default.Remove(RefreshKey);
            return Task.CompletedTask;
        }
    }
}
