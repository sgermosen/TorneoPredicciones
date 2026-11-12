using System.Threading.Tasks;
using CompeTournament.Mobile.Core.Services;
using CompeTournament.Shared.Auth;
using CompeTournament.Shared.Tournaments;
using Xunit;

namespace CompeTournament.Tests
{
    public class TournamentVisibilityTests : IClassFixture<ApiFactory>
    {
        private readonly ApiFactory _factory;

        public TournamentVisibilityTests(ApiFactory factory)
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
        public async Task PublicGroup_IsDiscoverable_PrivateIsNot()
        {
            var owner = await ClientAsync("jugador1@example.com", "Torneo2026");
            var pub = await owner.CreateGroupAsync(new CreateGroupRequest { Name = "Pública Demo", TournamentTypeId = 1, IsPublic = true });
            var priv = await owner.CreateGroupAsync(new CreateGroupRequest { Name = "Privada Demo", TournamentTypeId = 1, IsPublic = false });

            var stranger = await ClientAsync("jugador2@example.com", "Torneo2026");
            var visible = await stranger.GetGroupsAsync();

            Assert.Contains(visible, g => g.Id == pub.Id);
            Assert.DoesNotContain(visible, g => g.Id == priv.Id);
        }

        [Fact]
        public async Task PrivateGroup_RejectsOpenJoin()
        {
            var owner = await ClientAsync("jugador1@example.com", "Torneo2026");
            var priv = await owner.CreateGroupAsync(new CreateGroupRequest { Name = "Privada Join", TournamentTypeId = 1, IsPublic = false });

            var stranger = await ClientAsync("jugador2@example.com", "Torneo2026");

            await Assert.ThrowsAsync<ApiException>(() => stranger.JoinGroupAsync(priv.Id));
        }

        [Fact]
        public async Task Owner_TogglesToPrivate_RemovesFromDiscovery()
        {
            var owner = await ClientAsync("jugador1@example.com", "Torneo2026");
            var group = await owner.CreateGroupAsync(new CreateGroupRequest { Name = "Cambia Visibilidad", TournamentTypeId = 1, IsPublic = true });

            var stranger = await ClientAsync("jugador2@example.com", "Torneo2026");
            Assert.Contains(await stranger.GetGroupsAsync(), g => g.Id == group.Id);

            await owner.SetGroupVisibilityAsync(group.Id, new SetVisibilityRequest { IsPublic = false });

            Assert.DoesNotContain(await stranger.GetGroupsAsync(), g => g.Id == group.Id);
        }

        [Fact]
        public async Task NonOwner_CannotSetVisibility()
        {
            var owner = await ClientAsync("jugador1@example.com", "Torneo2026");
            var group = await owner.CreateGroupAsync(new CreateGroupRequest { Name = "Visibilidad Protegida", TournamentTypeId = 1, IsPublic = true });

            var stranger = await ClientAsync("jugador2@example.com", "Torneo2026");

            await Assert.ThrowsAsync<ApiException>(() => stranger.SetGroupVisibilityAsync(group.Id, new SetVisibilityRequest { IsPublic = false }));
        }
    }
}
