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

        private async Task<(ApiClient api, InMemoryTokenStore store, TokenResponse token)> AuthAsync()
        {
            var store = new InMemoryTokenStore();
            var api = new ApiClient(_factory.CreateClient(), store);
            var token = await api.LoginAsync(new TokenRequest { Username = "sgrysoft@gmail.com", Password = "Torneo2026" });
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
