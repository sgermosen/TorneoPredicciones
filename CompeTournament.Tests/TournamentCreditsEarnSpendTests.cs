using System;
using System.Threading.Tasks;
using CompeTournament.Mobile.Core.Services;
using CompeTournament.Shared.Auth;
using CompeTournament.Shared.Tournaments;
using Xunit;

namespace CompeTournament.Tests
{
    public class TournamentCreditsEarnSpendTests : IClassFixture<ApiFactory>
    {
        private readonly ApiFactory _factory;

        public TournamentCreditsEarnSpendTests(ApiFactory factory)
        {
            _factory = factory;
        }

        private async Task<ApiClient> ClientAsync(string email, string password)
        {
            var store = new InMemoryTokenStore();
            var client = new ApiClient(_factory.CreateClient(), store);
            var token = await client.LoginAsync(new TokenRequest { Username = email, Password = password });
            await store.SetTokensAsync(token.Token, token.RefreshToken);
            return client;
        }

        private static async Task<(int groupId, int matchId)> BuildMatchAsync(ApiClient owner, string name, bool creditsEnabled, int initial, int cost)
        {
            var group = await owner.CreateGroupAsync(new CreateGroupRequest { Name = name, TournamentTypeId = 1, CreditsEnabled = creditsEnabled, InitialCredits = initial, PredictionCost = cost });
            var league = await owner.CreateLeagueAsync(group.Id, new CreateLeagueRequest { Name = "Liga" });
            var local = await owner.CreateTeamAsync(group.Id, new CreateTeamRequest { LeagueId = league.Id, Name = "Tigres" });
            var visitor = await owner.CreateTeamAsync(group.Id, new CreateTeamRequest { LeagueId = league.Id, Name = "Leones" });
            var match = await owner.CreateMatchAsync(group.Id, new CreateMatchRequest { LocalId = local.TeamId, VisitorId = visitor.TeamId, DateTime = DateTime.UtcNow.AddDays(1) });
            return (group.Id, match.Id);
        }

        [Fact]
        public async Task Earn_OnClose_MirrorsPoints()
        {
            var owner = await ClientAsync("jugador1@example.com", "Torneo2026");
            var (groupId, matchId) = await BuildMatchAsync(owner, "Peña Gana", creditsEnabled: true, initial: 0, cost: 0);

            await owner.SavePredictionAsync(new PredictionRequest { MatchId = matchId, LocalPoints = 2, VisitorPoints = 1, IsBanker = false });
            await owner.CloseMatchAsync(matchId, new CloseMatchRequest { LocalPoints = 2, VisitorPoints = 1 });

            var detail = await owner.GetGroupAsync(groupId);
            Assert.Equal(3, detail.MyCredits);
        }

        [Fact]
        public async Task Predict_DebitsCost_OnlyOnce()
        {
            var owner = await ClientAsync("jugador1@example.com", "Torneo2026");
            var (groupId, matchId) = await BuildMatchAsync(owner, "Peña Cuesta", creditsEnabled: true, initial: 10, cost: 4);

            await owner.SavePredictionAsync(new PredictionRequest { MatchId = matchId, LocalPoints = 1, VisitorPoints = 0, IsBanker = false });
            Assert.Equal(6, (await owner.GetGroupAsync(groupId)).MyCredits);

            await owner.SavePredictionAsync(new PredictionRequest { MatchId = matchId, LocalPoints = 2, VisitorPoints = 2, IsBanker = false });
            Assert.Equal(6, (await owner.GetGroupAsync(groupId)).MyCredits);
        }

        [Fact]
        public async Task Predict_InsufficientCredits_Throws()
        {
            var owner = await ClientAsync("jugador1@example.com", "Torneo2026");
            var (groupId, matchId) = await BuildMatchAsync(owner, "Peña Pobre", creditsEnabled: true, initial: 2, cost: 5);

            await Assert.ThrowsAsync<ApiException>(() =>
                owner.SavePredictionAsync(new PredictionRequest { MatchId = matchId, LocalPoints = 1, VisitorPoints = 0, IsBanker = false }));
        }
    }
}
