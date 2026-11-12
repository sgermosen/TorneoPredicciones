using System.Linq;
using System.Threading.Tasks;
using CompeTournament.Mobile.Core.Services;
using CompeTournament.Shared.Auth;
using CompeTournament.Shared.Tournaments;
using Xunit;

namespace CompeTournament.Tests
{
    public class GroupManagementTests : IClassFixture<ApiFactory>
    {
        private readonly ApiFactory _factory;

        public GroupManagementTests(ApiFactory factory)
        {
            _factory = factory;
        }

        private async Task<(ApiClient Api, UserDto Me)> ClientAsync(string email, string password)
        {
            var store = new InMemoryTokenStore();
            var client = new ApiClient(_factory.CreateClient(), store);
            var token = await client.LoginAsync(new TokenRequest { Username = email, Password = password });
            await store.SetTokensAsync(token.Token, token.RefreshToken);
            var me = await client.GetMeAsync();
            return (client, me);
        }

        [Fact]
        public async Task Owner_SeesPendingMember_AndAccepts()
        {
            var (owner, _) = await ClientAsync("jugador1@example.com", "Torneo2026");
            var (member, memberMe) = await ClientAsync("jugador2@example.com", "Torneo2026");

            var group = await owner.CreateGroupAsync(new CreateGroupRequest { Name = "Peña Aceptar", TournamentTypeId = 1 });
            await member.JoinGroupAsync(group.Id);

            var pending = (await owner.GetGroupMembersAsync(group.Id)).Single(m => m.UserId == memberMe.Id);
            Assert.False(pending.IsAccepted);
            Assert.False(pending.IsBlocked);

            await owner.AcceptMemberAsync(group.Id, memberMe.Id);

            Assert.True((await owner.GetGroupMembersAsync(group.Id)).Single(m => m.UserId == memberMe.Id).IsAccepted);
            Assert.Contains(await owner.GetLeaderboardAsync(group.Id), m => m.UserId == memberMe.Id);
        }

        [Fact]
        public async Task Owner_BlocksMember_RemovedFromLeaderboard()
        {
            var (owner, _) = await ClientAsync("jugador1@example.com", "Torneo2026");
            var (member, memberMe) = await ClientAsync("jugador2@example.com", "Torneo2026");

            var group = await owner.CreateGroupAsync(new CreateGroupRequest { Name = "Peña Bloquear", TournamentTypeId = 1 });
            await member.JoinGroupAsync(group.Id);
            await owner.AcceptMemberAsync(group.Id, memberMe.Id);

            await owner.BlockMemberAsync(group.Id, memberMe.Id);

            Assert.True((await owner.GetGroupMembersAsync(group.Id)).Single(m => m.UserId == memberMe.Id).IsBlocked);
            Assert.DoesNotContain(await owner.GetLeaderboardAsync(group.Id), m => m.UserId == memberMe.Id);
        }

        [Fact]
        public async Task NonOwnerDenied_ButGlobalAdminAllowed()
        {
            var (owner, _) = await ClientAsync("jugador1@example.com", "Torneo2026");
            var (member, memberMe) = await ClientAsync("jugador2@example.com", "Torneo2026");
            var (admin, _) = await ClientAsync("admin@example.com", "Torneo2026");

            var group = await owner.CreateGroupAsync(new CreateGroupRequest { Name = "Peña Ajena", TournamentTypeId = 1 });
            await member.JoinGroupAsync(group.Id);

            await Assert.ThrowsAsync<ApiException>(() => member.GetGroupMembersAsync(group.Id));
            await Assert.ThrowsAsync<ApiException>(() => member.AcceptMemberAsync(group.Id, memberMe.Id));
            await Assert.ThrowsAsync<ApiException>(() => member.BlockMemberAsync(group.Id, memberMe.Id));

            Assert.Contains(await admin.GetGroupMembersAsync(group.Id), m => m.UserId == memberMe.Id);
            await admin.AcceptMemberAsync(group.Id, memberMe.Id);
            Assert.True((await admin.GetGroupMembersAsync(group.Id)).Single(m => m.UserId == memberMe.Id).IsAccepted);
        }
    }
}
