using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CompeTournament.Mobile.Core.Services;
using CompeTournament.Shared.Auth;
using CompeTournament.Shared.Tournaments;
using Xunit;

namespace CompeTournament.Tests
{
    public class DuelsTests : IClassFixture<ApiFactory>
    {
        private readonly ApiFactory _factory;

        public DuelsTests(ApiFactory factory)
        {
            _factory = factory;
        }

        private async Task<(ApiClient api, string userId)> ClientAsync(string email)
        {
            var store = new InMemoryTokenStore();
            var api = new ApiClient(_factory.CreateClient(), store);
            var token = await api.LoginAsync(new TokenRequest { Username = email, Password = "Torneo2026" });
            await store.SetTokensAsync(token.Token, token.RefreshToken);
            return (api, token.User.Id);
        }

        private static async Task<(int groupId, List<int> matchIds)> DemoAsync(ApiClient api)
        {
            var groups = await api.GetGroupsAsync();
            var demo = groups.First(g => g.Name.Contains("Demo"));
            var detail = await api.GetGroupAsync(demo.Id);
            return (demo.Id, detail.Matches.Select(m => m.Id).ToList());
        }

        [Fact]
        public async Task CreateDuel_IsPending_AndOpponentCanRespond()
        {
            var (j1, _) = await ClientAsync("jugador1@example.com");
            var (j2, j2Id) = await ClientAsync("jugador2@example.com");
            var (groupId, matches) = await DemoAsync(j1);

            var created = await j1.CreateDuelAsync(groupId, new CreateDuelRequest { OpponentUserId = j2Id, MatchId = matches[0] });
            Assert.Equal("Pending", created.Status);
            Assert.False(created.CanRespond);

            var opponentDuels = await j2.GetMyDuelsAsync(groupId);
            Assert.Contains(opponentDuels, d => d.Id == created.Id && d.CanRespond);
        }

        [Fact]
        public async Task Accept_ThenClose_ResolvesWinner()
        {
            var (j1, _) = await ClientAsync("jugador1@example.com");
            var (j2, j2Id) = await ClientAsync("jugador2@example.com");
            var (admin, _) = await ClientAsync("admin@example.com");
            var (groupId, matches) = await DemoAsync(j1);
            var matchId = matches[1];

            var created = await j1.CreateDuelAsync(groupId, new CreateDuelRequest { OpponentUserId = j2Id, MatchId = matchId });
            await j2.AcceptDuelAsync(groupId, created.Id);

            await j1.SavePredictionAsync(new PredictionRequest { MatchId = matchId, LocalPoints = 2, VisitorPoints = 1 });
            await j2.SavePredictionAsync(new PredictionRequest { MatchId = matchId, LocalPoints = 0, VisitorPoints = 0 });
            await admin.CloseMatchAsync(matchId, new CloseMatchRequest { LocalPoints = 2, VisitorPoints = 1 });

            var duel = (await j1.GetMyDuelsAsync(groupId)).First(d => d.Id == created.Id);
            Assert.Equal("Accepted", duel.Status);
            Assert.True(duel.MatchClosed);
            Assert.Equal(3, duel.ChallengerPoints);
            Assert.Equal(0, duel.OpponentPoints);
            Assert.Equal(duel.ChallengerName, duel.WinnerName);
        }

        [Fact]
        public async Task Decline_SetsDeclined()
        {
            var (j1, _) = await ClientAsync("jugador1@example.com");
            var (j2, j2Id) = await ClientAsync("jugador2@example.com");
            var (groupId, matches) = await DemoAsync(j1);

            var created = await j1.CreateDuelAsync(groupId, new CreateDuelRequest { OpponentUserId = j2Id, MatchId = matches[2] });
            await j2.DeclineDuelAsync(groupId, created.Id);

            var duel = (await j1.GetMyDuelsAsync(groupId)).First(d => d.Id == created.Id);
            Assert.Equal("Declined", duel.Status);
        }

        [Fact]
        public async Task CannotChallengeSelf()
        {
            var (j1, j1Id) = await ClientAsync("jugador1@example.com");
            var (groupId, matches) = await DemoAsync(j1);

            await Assert.ThrowsAsync<ApiException>(() =>
                j1.CreateDuelAsync(groupId, new CreateDuelRequest { OpponentUserId = j1Id, MatchId = matches[0] }));
        }
    }
}
