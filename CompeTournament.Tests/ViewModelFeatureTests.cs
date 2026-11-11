using System.Linq;
using System.Threading.Tasks;
using CompeTournament.Mobile.Core.Services;
using CompeTournament.Mobile.Core.ViewModels;
using CompeTournament.Shared.Auth;
using Xunit;

namespace CompeTournament.Tests
{
    public class ViewModelFeatureTests : IClassFixture<ApiFactory>
    {
        private readonly ApiFactory _factory;

        public ViewModelFeatureTests(ApiFactory factory)
        {
            _factory = factory;
        }

        private Task<(ApiClient api, InMemoryTokenStore store, TokenResponse token)> AuthAsync() => AuthAsAsync("admin@example.com");

        private async Task<(ApiClient api, InMemoryTokenStore store, TokenResponse token)> AuthAsAsync(string email)
        {
            var store = new InMemoryTokenStore();
            var api = new ApiClient(_factory.CreateClient(), store);
            var token = await api.LoginAsync(new TokenRequest { Username = email, Password = "Torneo2026" });
            await store.SetTokensAsync(token.Token, token.RefreshToken);
            return (api, store, token);
        }

        [Fact]
        public async Task GroupDetail_LoadsRecapAndInviteCode()
        {
            var (api, _, _) = await AuthAsync();
            var vm = new GroupDetailViewModel(api, new FakeNavigationService());

            await vm.InitializeAsync(1);

            Assert.False(string.IsNullOrWhiteSpace(vm.Recap));
            Assert.Equal("COPA2026", vm.InviteCode);
            Assert.NotEmpty(vm.Leaderboard);
        }

        [Fact]
        public async Task MatchPrediction_LoadsAndPostsComment()
        {
            var (api, _, _) = await AuthAsync();
            var vm = new MatchPredictionViewModel(api, new FakeNavigationService());

            await vm.InitializeAsync(1);
            Assert.NotNull(vm.Match);

            vm.NewCommentBody = "Vamos Tigres!";
            await vm.PostCommentCommand.ExecuteAsync(null);

            Assert.Contains(vm.Comments, c => c.Body == "Vamos Tigres!");
            Assert.Equal(string.Empty, vm.NewCommentBody);
        }

        [Fact]
        public async Task Groups_JoinByCode_JoinsAndShowsMembership()
        {
            var (api, _, _) = await AuthAsAsync("jugador2@example.com");
            var vm = new GroupsViewModel(api, new FakeNavigationService());

            vm.InviteCode = "copa2026";
            await vm.JoinByCodeCommand.ExecuteAsync(null);

            Assert.Null(vm.ErrorMessage);
            Assert.Equal("Te uniste a Copa Amistosa 2026.", vm.SuccessMessage);
            Assert.Equal(string.Empty, vm.InviteCode);
            Assert.Contains(vm.Groups, g => g.Name == "Copa Amistosa 2026" && g.IsMember && g.IsAccepted);
        }

        [Fact]
        public async Task Groups_JoinByCode_InvalidCode_ShowsServerMessage()
        {
            var (api, _, _) = await AuthAsAsync("jugador2@example.com");
            var vm = new GroupsViewModel(api, new FakeNavigationService());

            vm.InviteCode = "NOEXISTE";
            await vm.JoinByCodeCommand.ExecuteAsync(null);

            Assert.Equal("Código de invitación inválido.", vm.ErrorMessage);
            Assert.Null(vm.SuccessMessage);
        }

        [Fact]
        public async Task GroupDetail_JoinRequest_StaysPendingUntilAccepted()
        {
            var (api, _, _) = await AuthAsAsync("organizador@example.com");
            var vm = new GroupDetailViewModel(api, new FakeNavigationService());
            await vm.InitializeAsync(1);
            Assert.True(vm.CanJoin);
            Assert.False(vm.IsPending);

            await vm.JoinCommand.ExecuteAsync(null);

            Assert.False(vm.CanJoin);
            Assert.True(vm.IsPending);
        }

        [Fact]
        public async Task Profile_Refresh_LoadsInsights()
        {
            var (api, store, token) = await AuthAsync();
            var session = new Session();
            session.SetUser(token.User);

            var vm = new ProfileViewModel(api, store, session, new FakeNavigationService());
            await vm.RefreshCommand.ExecuteAsync(null);

            Assert.NotNull(vm.Insights);
        }
    }
}
