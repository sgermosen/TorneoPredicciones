using System.Threading.Tasks;
using CompeTournament.Mobile.Core.Services;
using CompeTournament.Shared.Auth;
using Xunit;

namespace CompeTournament.Tests
{
    public class AuthRefreshTests : IClassFixture<ApiFactory>
    {
        private readonly ApiFactory _factory;

        public AuthRefreshTests(ApiFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task Refresh_RotatesToken_AndRevokesOld()
        {
            var http = _factory.CreateClient();
            var client = new ApiClient(http, new InMemoryTokenStore());

            var login = await client.LoginAsync(new TokenRequest
            {
                Username = "sgrysoft@gmail.com",
                Password = "Torneo2026"
            });
            Assert.False(string.IsNullOrWhiteSpace(login.RefreshToken));

            var refreshed = await client.RefreshAsync(login.RefreshToken);
            Assert.False(string.IsNullOrWhiteSpace(refreshed.Token));
            Assert.NotEqual(login.RefreshToken, refreshed.RefreshToken);

            await Assert.ThrowsAsync<ApiException>(() => client.RefreshAsync(login.RefreshToken));
        }

        [Fact]
        public async Task ApiClient_AutoRefreshes_OnExpiredAccessToken()
        {
            var http = _factory.CreateClient();
            var store = new InMemoryTokenStore();
            var client = new ApiClient(http, store);

            var login = await client.LoginAsync(new TokenRequest
            {
                Username = "sgrysoft@gmail.com",
                Password = "Torneo2026"
            });

            await store.SetTokensAsync("invalid.access.token", login.RefreshToken);

            var groups = await client.GetGroupsAsync();
            Assert.NotEmpty(groups);

            var currentToken = await store.GetTokenAsync();
            Assert.NotEqual("invalid.access.token", currentToken);
        }
    }
}
