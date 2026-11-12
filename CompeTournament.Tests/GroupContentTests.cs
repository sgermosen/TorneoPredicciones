using System;
using System.Threading.Tasks;
using CompeTournament.Mobile.Core.Services;
using CompeTournament.Shared.Auth;
using CompeTournament.Shared.Tournaments;
using Xunit;

namespace CompeTournament.Tests
{
    public class GroupContentTests : IClassFixture<ApiFactory>
    {
        private readonly ApiFactory _factory;

        public GroupContentTests(ApiFactory factory)
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
        public async Task Owner_BuildsLeagueTeamsMatch_AndCloses()
        {
            var owner = await ClientAsync("jugador1@example.com", "Torneo2026");

            var group = await owner.CreateGroupAsync(new CreateGroupRequest { Name = "Peña Contenido", TournamentTypeId = 1 });

            var league = await owner.CreateLeagueAsync(group.Id, new CreateLeagueRequest { Name = "Liga Única" });
            Assert.True(league.Id > 0);

            var local = await owner.CreateTeamAsync(group.Id, new CreateTeamRequest { LeagueId = league.Id, Name = "Tigres" });
            var visitor = await owner.CreateTeamAsync(group.Id, new CreateTeamRequest { LeagueId = league.Id, Name = "Leones" });
            Assert.True(local.TeamId > 0);
            Assert.True(visitor.TeamId > 0);
            Assert.Equal("TIG", local.Initials);

            var match = await owner.CreateMatchAsync(group.Id, new CreateMatchRequest
            {
                LocalId = local.TeamId,
                VisitorId = visitor.TeamId,
                DateTime = DateTime.UtcNow.AddDays(1)
            });
            Assert.True(match.Id > 0);
            Assert.True(match.IsOpen);
            Assert.Equal(local.TeamId, match.LocalId);

            await owner.CloseMatchAsync(match.Id, new CloseMatchRequest { LocalPoints = 2, VisitorPoints = 1 });

            var afterClose = await owner.GetMatchAsync(match.Id);
            Assert.False(afterClose.IsOpen);
        }

        [Fact]
        public async Task CreateMatch_PastDate_Throws()
        {
            var owner = await ClientAsync("jugador1@example.com", "Torneo2026");

            var group = await owner.CreateGroupAsync(new CreateGroupRequest { Name = "Peña Fecha", TournamentTypeId = 1 });
            var league = await owner.CreateLeagueAsync(group.Id, new CreateLeagueRequest { Name = "Liga" });
            var a = await owner.CreateTeamAsync(group.Id, new CreateTeamRequest { LeagueId = league.Id, Name = "Aguilas" });
            var b = await owner.CreateTeamAsync(group.Id, new CreateTeamRequest { LeagueId = league.Id, Name = "Buhos" });

            await Assert.ThrowsAsync<ApiException>(() => owner.CreateMatchAsync(group.Id, new CreateMatchRequest
            {
                LocalId = a.TeamId,
                VisitorId = b.TeamId,
                DateTime = DateTime.UtcNow.AddDays(-1)
            }));
        }

        [Fact]
        public async Task NonOwner_CannotCreateContent()
        {
            var owner = await ClientAsync("jugador1@example.com", "Torneo2026");
            var stranger = await ClientAsync("jugador2@example.com", "Torneo2026");

            var group = await owner.CreateGroupAsync(new CreateGroupRequest { Name = "Peña Cerrada", TournamentTypeId = 1 });
            var league = await owner.CreateLeagueAsync(group.Id, new CreateLeagueRequest { Name = "Liga" });

            await Assert.ThrowsAsync<ApiException>(() => stranger.CreateLeagueAsync(group.Id, new CreateLeagueRequest { Name = "Intrusa" }));
            await Assert.ThrowsAsync<ApiException>(() => stranger.CreateTeamAsync(group.Id, new CreateTeamRequest { LeagueId = league.Id, Name = "Equipo" }));
            await Assert.ThrowsAsync<ApiException>(() => stranger.CreateMatchAsync(group.Id, new CreateMatchRequest { LocalId = 1, VisitorId = 2, DateTime = DateTime.UtcNow.AddDays(1) }));
        }

        [Fact]
        public async Task GlobalAdmin_CanCloseOwnersMatch()
        {
            var owner = await ClientAsync("jugador1@example.com", "Torneo2026");
            var admin = await ClientAsync("admin@example.com", "Torneo2026");

            var group = await owner.CreateGroupAsync(new CreateGroupRequest { Name = "Peña Admin Cierra", TournamentTypeId = 1 });
            var league = await owner.CreateLeagueAsync(group.Id, new CreateLeagueRequest { Name = "Liga" });
            var local = await owner.CreateTeamAsync(group.Id, new CreateTeamRequest { LeagueId = league.Id, Name = "Rayos" });
            var visitor = await owner.CreateTeamAsync(group.Id, new CreateTeamRequest { LeagueId = league.Id, Name = "Truenos" });
            var match = await owner.CreateMatchAsync(group.Id, new CreateMatchRequest
            {
                LocalId = local.TeamId,
                VisitorId = visitor.TeamId,
                DateTime = DateTime.UtcNow.AddDays(1)
            });

            await admin.CloseMatchAsync(match.Id, new CloseMatchRequest { LocalPoints = 0, VisitorPoints = 0 });

            var afterClose = await owner.GetMatchAsync(match.Id);
            Assert.False(afterClose.IsOpen);
        }
    }
}
