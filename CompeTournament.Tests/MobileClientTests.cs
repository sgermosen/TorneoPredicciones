using System.Linq;
using System.Threading.Tasks;
using CompeTournament.Mobile.Core;
using CompeTournament.Mobile.Core.Services;
using CompeTournament.Mobile.Core.ViewModels;
using Xunit;

namespace CompeTournament.Tests
{
    public class MobileClientTests : IClassFixture<ApiFactory>
    {
        private readonly ApiFactory _factory;

        public MobileClientTests(ApiFactory factory)
        {
            _factory = factory;
        }

        private (ApiClient client, InMemoryTokenStore store) CreateClient()
        {
            var http = _factory.CreateClient();
            var store = new InMemoryTokenStore();
            return (new ApiClient(http, store), store);
        }

        [Fact]
        public async Task ApiClient_LoginAndReadGroups()
        {
            var (client, store) = CreateClient();

            var token = await client.LoginAsync(new CompeTournament.Shared.Auth.TokenRequest
            {
                Username = "sgrysoft@gmail.com",
                Password = "Torneo2026"
            });
            await store.SetTokenAsync(token.Token);

            var groups = await client.GetGroupsAsync();
            Assert.NotEmpty(groups);

            var detail = await client.GetGroupAsync(groups.First().Id);
            Assert.NotEmpty(detail.Matches);
        }

        [Fact]
        public async Task ApiClient_UnauthenticatedGroups_Throws()
        {
            var (client, _) = CreateClient();
            await Assert.ThrowsAsync<ApiException>(() => client.GetGroupsAsync());
        }

        [Fact]
        public async Task LoginViewModel_SuccessfulLogin_SetsSessionAndNavigates()
        {
            var http = _factory.CreateClient();
            var store = new InMemoryTokenStore();
            var apiClient = new ApiClient(http, store);
            var session = new Session();
            var navigation = new FakeNavigationService();

            var vm = new LoginViewModel(apiClient, store, session, navigation)
            {
                Email = "sgrysoft@gmail.com",
                Password = "Torneo2026"
            };

            await vm.LoginCommand.ExecuteAsync(null);

            Assert.True(session.IsAuthenticated);
            Assert.Equal("sgrysoft@gmail.com", session.CurrentUser!.Email);
            Assert.Contains(navigation.Routes, r => r.Contains(AppRoutes.Groups));
            Assert.Null(vm.ErrorMessage);
        }

        [Fact]
        public async Task LoginViewModel_WrongPassword_ShowsError()
        {
            var http = _factory.CreateClient();
            var store = new InMemoryTokenStore();
            var apiClient = new ApiClient(http, store);
            var session = new Session();
            var navigation = new FakeNavigationService();

            var vm = new LoginViewModel(apiClient, store, session, navigation)
            {
                Email = "sgrysoft@gmail.com",
                Password = "wrong-password"
            };

            await vm.LoginCommand.ExecuteAsync(null);

            Assert.False(session.IsAuthenticated);
            Assert.NotNull(vm.ErrorMessage);
        }

        [Fact]
        public async Task GroupDetailViewModel_LoadsMatchesStandingsAndLeaderboard()
        {
            var http = _factory.CreateClient();
            var store = new InMemoryTokenStore();
            var apiClient = new ApiClient(http, store);

            var token = await apiClient.LoginAsync(new CompeTournament.Shared.Auth.TokenRequest
            {
                Username = "sgrysoft@gmail.com",
                Password = "Torneo2026"
            });
            await store.SetTokenAsync(token.Token);

            var navigation = new FakeNavigationService();
            var vm = new GroupDetailViewModel(apiClient, navigation);
            await vm.InitializeAsync(1);

            Assert.NotNull(vm.Group);
            Assert.NotEmpty(vm.Matches);
            Assert.NotEmpty(vm.Standings);
            Assert.NotEmpty(vm.Leaderboard);
        }
    }
}
