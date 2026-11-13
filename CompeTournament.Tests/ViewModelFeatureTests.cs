using System.Linq;
using System.Threading.Tasks;
using CompeTournament.Mobile.Core.Services;
using CompeTournament.Mobile.Core.ViewModels;
using CompeTournament.Shared.Auth;
using CompeTournament.Shared.Tournaments;
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
        public async Task Groups_PendingDeepLink_AutoJoins()
        {
            var (api, _, _) = await AuthAsAsync("organizador@example.com");
            var vm = new GroupsViewModel(api, new FakeNavigationService());
            AppLinkState.PendingInviteCode = "copa2026";

            await vm.TryJoinPendingAsync();

            Assert.Null(AppLinkState.PendingInviteCode);
            Assert.Equal("Te uniste a Copa Amistosa 2026.", vm.SuccessMessage);
            Assert.Contains(vm.Groups, g => g.Name == "Copa Amistosa 2026" && g.IsMember);
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
        public void GroupDetail_ShareText_IncludesRankingAndCode()
        {
            var vm = new GroupDetailViewModel(null, new FakeNavigationService());
            vm.Group = new GroupDetailDto { Name = "Peña Test" };
            vm.Leaderboard.Add(new LeaderboardEntryDto { FullName = "Ana", Points = 9 });
            vm.Leaderboard.Add(new LeaderboardEntryDto { FullName = "Luis", Points = 4 });
            vm.InviteCode = "ABC123";

            var text = vm.BuildShareText();

            Assert.Contains("Peña Test", text);
            Assert.Contains("1. Ana - 9 pts", text);
            Assert.Contains("2. Luis - 4 pts", text);
            Assert.Contains("ABC123", text);
        }

        [Fact]
        public void ProfileBadges_ReflectInsights()
        {
            Assert.Empty(ProfileViewModel.BuildBadges(null));
            Assert.Empty(ProfileViewModel.BuildBadges(new InsightsDto { TotalResolved = 0 }));

            var badges = ProfileViewModel.BuildBadges(new InsightsDto
            {
                TotalResolved = 12,
                ExactHits = 6,
                OutcomeHits = 2,
                Accuracy = 0.75,
                CurrentStreak = 4,
                BestStreak = 6,
                TotalPoints = 120
            });

            Assert.Contains(badges, b => b.Name == "Primer acierto");
            Assert.Contains(badges, b => b.Name == "Francotirador");
            Assert.Contains(badges, b => b.Name == "En racha");
            Assert.Contains(badges, b => b.Name == "Imparable");
            Assert.Contains(badges, b => b.Name == "Centenario");
            Assert.Contains(badges, b => b.Name == "Experto");
        }

        [Fact]
        public void GroupDetail_RuleTexts_ReflectGroupSettings()
        {
            var vm = new GroupDetailViewModel(null, new FakeNavigationService());

            vm.Group = new GroupDetailDto { IsPublic = false, CreditsEnabled = true, PredictionCost = 2 };
            Assert.Equal("Torneo privado: solo por invitación.", vm.VisibilityText);
            Assert.Equal("Cada predicción cuesta 2 fichas.", vm.PredictionCostText);

            vm.Group = new GroupDetailDto { IsPublic = true, CreditsEnabled = true, PredictionCost = 0 };
            Assert.Equal("Torneo público: cualquiera puede unirse.", vm.VisibilityText);
            Assert.Equal("Predecir es gratis.", vm.PredictionCostText);

            vm.Group = new GroupDetailDto { CreditsEnabled = false };
            Assert.Null(vm.PredictionCostText);
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
