using System;
using System.Threading.Tasks;
using CompeTournament.Mobile.Core.Services;
using CompeTournament.Shared.Auth;
using CompeTournament.Shared.Tournaments;
using Xunit;

namespace CompeTournament.Tests
{
    public class TournamentNestedTests : IClassFixture<ApiFactory>
    {
        private readonly ApiFactory _factory;

        public TournamentNestedTests(ApiFactory factory)
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

        [Fact]
        public async Task Owner_CreatesInternalTournament_AndAssignsMatch()
        {
            var owner = await ClientAsync("jugador1@example.com", "Torneo2026");
            var group = await owner.CreateGroupAsync(new CreateGroupRequest { Name = "Peña Torneos", TournamentTypeId = 1 });

            var tournament = await owner.CreateTournamentAsync(group.Id, new CreateTournamentRequest { Name = "Apertura" });
            Assert.True(tournament.Id > 0);

            var league = await owner.CreateLeagueAsync(group.Id, new CreateLeagueRequest { Name = "Liga" });
            var local = await owner.CreateTeamAsync(group.Id, new CreateTeamRequest { LeagueId = league.Id, Name = "Tigres" });
            var visitor = await owner.CreateTeamAsync(group.Id, new CreateTeamRequest { LeagueId = league.Id, Name = "Leones" });

            var match = await owner.CreateMatchAsync(group.Id, new CreateMatchRequest
            {
                LocalId = local.TeamId,
                VisitorId = visitor.TeamId,
                DateTime = DateTime.UtcNow.AddDays(1),
                TournamentId = tournament.Id
            });

            Assert.Equal(tournament.Id, match.TournamentId);

            var detail = await owner.GetGroupAsync(group.Id);
            Assert.Contains(detail.Tournaments, t => t.Id == tournament.Id && t.Name == "Apertura");
            Assert.Contains(detail.Matches, m => m.Id == match.Id && m.TournamentId == tournament.Id);

            var tournaments = await owner.GetGroupTournamentsAsync(group.Id);
            Assert.Contains(tournaments, t => t.Id == tournament.Id);
        }

        [Fact]
        public async Task CreateMatch_WithTournamentFromAnotherGroup_Throws()
        {
            var owner = await ClientAsync("jugador1@example.com", "Torneo2026");

            var g1 = await owner.CreateGroupAsync(new CreateGroupRequest { Name = "Peña Uno", TournamentTypeId = 1 });
            var l1 = await owner.CreateLeagueAsync(g1.Id, new CreateLeagueRequest { Name = "Liga" });
            var a = await owner.CreateTeamAsync(g1.Id, new CreateTeamRequest { LeagueId = l1.Id, Name = "Aguilas" });
            var b = await owner.CreateTeamAsync(g1.Id, new CreateTeamRequest { LeagueId = l1.Id, Name = "Buhos" });

            var g2 = await owner.CreateGroupAsync(new CreateGroupRequest { Name = "Peña Dos", TournamentTypeId = 1 });
            var foreign = await owner.CreateTournamentAsync(g2.Id, new CreateTournamentRequest { Name = "Ajeno" });

            await Assert.ThrowsAsync<ApiException>(() => owner.CreateMatchAsync(g1.Id, new CreateMatchRequest
            {
                LocalId = a.TeamId,
                VisitorId = b.TeamId,
                DateTime = DateTime.UtcNow.AddDays(1),
                TournamentId = foreign.Id
            }));
        }
    }
}
