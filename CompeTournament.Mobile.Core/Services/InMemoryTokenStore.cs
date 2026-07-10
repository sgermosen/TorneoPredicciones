namespace CompeTournament.Mobile.Core.Services
{
    public class InMemoryTokenStore : ITokenStore
    {
        private string? _token;

        public Task<string?> GetTokenAsync() => Task.FromResult(_token);

        public Task SetTokenAsync(string token)
        {
            _token = token;
            return Task.CompletedTask;
        }

        public Task ClearAsync()
        {
            _token = null;
            return Task.CompletedTask;
        }
    }
}
