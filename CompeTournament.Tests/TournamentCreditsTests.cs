using System.Threading.Tasks;
using CompeTournament.Mobile.Core.Services;
using CompeTournament.Shared.Auth;
using CompeTournament.Shared.Tournaments;
using Xunit;

namespace CompeTournament.Tests
{
    public class TournamentCreditsTests : IClassFixture<ApiFactory>
    {
        private readonly ApiFactory _factory;

        public TournamentCreditsTests(ApiFactory factory)
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
        public async Task CreditsEnabled_GrantsInitialOnCreateAndInvite()
        {
            var owner = await ClientAsync("jugador1@example.com", "Torneo2026");
            var group = await owner.CreateGroupAsync(new CreateGroupRequest { Name = "Peña Fichas", TournamentTypeId = 1, CreditsEnabled = true, InitialCredits = 50 });

            var detail = await owner.GetGroupAsync(group.Id);
            Assert.True(detail.CreditsEnabled);
            Assert.Equal(50, detail.MyCredits);

            var invited = await owner.InviteMemberAsync(group.Id, new InviteMemberRequest { Email = "jugador2@example.com" });
            Assert.Equal(50, invited.Credits);
        }

        [Fact]
        public async Task CreditsDisabled_NoGrant()
        {
            var owner = await ClientAsync("jugador1@example.com", "Torneo2026");
            var group = await owner.CreateGroupAsync(new CreateGroupRequest { Name = "Peña Sin Fichas", TournamentTypeId = 1 });

            var detail = await owner.GetGroupAsync(group.Id);
            Assert.False(detail.CreditsEnabled);
            Assert.Equal(0, detail.MyCredits);
        }

        [Fact]
        public async Task Owner_AdjustsMemberCredits_AndCannotGoNegative()
        {
            var owner = await ClientAsync("jugador1@example.com", "Torneo2026");
            var group = await owner.CreateGroupAsync(new CreateGroupRequest { Name = "Peña Ajuste", TournamentTypeId = 1, CreditsEnabled = true, InitialCredits = 50 });
            var member = await owner.InviteMemberAsync(group.Id, new InviteMemberRequest { Email = "jugador2@example.com" });

            var after = await owner.AdjustCreditsAsync(group.Id, member.UserId, new AdjustCreditsRequest { Amount = 25 });
            Assert.Equal(75, after.Credits);

            await Assert.ThrowsAsync<ApiException>(() => owner.AdjustCreditsAsync(group.Id, member.UserId, new AdjustCreditsRequest { Amount = -1000 }));
        }
    }
}
