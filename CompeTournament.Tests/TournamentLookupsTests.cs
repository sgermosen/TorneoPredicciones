using System.Threading.Tasks;
using CompeTournament.Mobile.Core.Services;
using CompeTournament.Shared.Auth;
using CompeTournament.Shared.Tournaments;
using Xunit;

namespace CompeTournament.Tests
{
    public class TournamentLookupsTests : IClassFixture<ApiFactory>
    {
        private readonly ApiFactory _factory;

        public TournamentLookupsTests(ApiFactory factory)
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
        public async Task AnyUser_GetsTournamentTypes()
        {
            var user = await ClientAsync("jugador1@example.com", "Torneo2026");

            var types = await user.GetTournamentTypesAsync();

            Assert.NotEmpty(types);
            Assert.Contains(types, t => t.Name != null && t.Name.StartsWith("Por marcador"));
            Assert.All(types, t => Assert.True(t.Id > 0));
        }

        [Fact]
        public async Task Owner_GetsGroupLeagues()
        {
            var owner = await ClientAsync("jugador1@example.com", "Torneo2026");

            var group = await owner.CreateGroupAsync(new CreateGroupRequest { Name = "Peña Lecturas", TournamentTypeId = 1 });
            var league = await owner.CreateLeagueAsync(group.Id, new CreateLeagueRequest { Name = "Liga Lecturas" });

            var leagues = await owner.GetGroupLeaguesAsync(group.Id);

            Assert.Contains(leagues, l => l.Id == league.Id && l.Name == "Liga Lecturas");
        }

        [Fact]
        public async Task NonOwner_CannotGetGroupLeagues()
        {
            var owner = await ClientAsync("jugador1@example.com", "Torneo2026");
            var stranger = await ClientAsync("jugador2@example.com", "Torneo2026");

            var group = await owner.CreateGroupAsync(new CreateGroupRequest { Name = "Peña Privada Lecturas", TournamentTypeId = 1 });
            await owner.CreateLeagueAsync(group.Id, new CreateLeagueRequest { Name = "Liga" });

            await Assert.ThrowsAsync<ApiException>(() => stranger.GetGroupLeaguesAsync(group.Id));
        }
    }
}
