using CompeTournament.Mobile.Core.Services;

namespace CompeTournament.Mobile.Services
{
    public class SecureStorageTokenStore : ITokenStore
    {
        private const string TokenKey = "competournament_auth_token";

        public Task<string?> GetTokenAsync() => SecureStorage.Default.GetAsync(TokenKey);

        public Task SetTokenAsync(string token) => SecureStorage.Default.SetAsync(TokenKey, token);

        public Task ClearAsync()
        {
            SecureStorage.Default.Remove(TokenKey);
            return Task.CompletedTask;
        }
    }
}
