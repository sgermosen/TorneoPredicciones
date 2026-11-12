using System.Threading.Tasks;
using CompeTournament.Mobile.Core.Services;
using CompeTournament.Shared.Auth;
using CompeTournament.Shared.Tournaments;
using Xunit;

namespace CompeTournament.Tests
{
    public class TournamentInviteTests : IClassFixture<ApiFactory>
    {
        private readonly ApiFactory _factory;

        public TournamentInviteTests(ApiFactory factory)
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
        public async Task Owner_InvitesExistingUser_ByEmail_AddsAcceptedMember()
        {
            var owner = await ClientAsync("jugador1@example.com", "Torneo2026");
            var group = await owner.CreateGroupAsync(new CreateGroupRequest { Name = "Peña Invita", TournamentTypeId = 1 });

            var member = await owner.InviteMemberAsync(group.Id, new InviteMemberRequest { Email = "jugador2@example.com" });

            Assert.True(member.IsAccepted);
            Assert.False(member.IsBlocked);
            Assert.False(string.IsNullOrEmpty(member.UserId));

            var members = await owner.GetGroupMembersAsync(group.Id);
            Assert.Equal(2, members.Count);
        }

        [Fact]
        public async Task Invite_UnknownEmail_Throws()
        {
            var owner = await ClientAsync("jugador1@example.com", "Torneo2026");
            var group = await owner.CreateGroupAsync(new CreateGroupRequest { Name = "Peña Invita 2", TournamentTypeId = 1 });

            await Assert.ThrowsAsync<ApiException>(() =>
                owner.InviteMemberAsync(group.Id, new InviteMemberRequest { Email = "nadie-de-nadie@example.com" }));
        }

        [Fact]
        public async Task NonOwner_CannotInvite()
        {
            var owner = await ClientAsync("jugador1@example.com", "Torneo2026");
            var stranger = await ClientAsync("jugador2@example.com", "Torneo2026");
            var group = await owner.CreateGroupAsync(new CreateGroupRequest { Name = "Peña Invita 3", TournamentTypeId = 1 });

            await Assert.ThrowsAsync<ApiException>(() =>
                stranger.InviteMemberAsync(group.Id, new InviteMemberRequest { Email = "admin@example.com" }));
        }
    }
}
