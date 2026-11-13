using System.Linq;
using System.Threading.Tasks;
using CompeTournament.Mobile.Core.Services;
using CompeTournament.Shared.Auth;
using CompeTournament.Shared.Tournaments;
using Xunit;

namespace CompeTournament.Tests
{
    public class CreditHistoryTests : IClassFixture<ApiFactory>
    {
        private readonly ApiFactory _factory;

        public CreditHistoryTests(ApiFactory factory)
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
        public async Task History_ReturnsInitialGrant_ForMember()
        {
            var owner = await ClientAsync("jugador1@example.com", "Torneo2026");
            var group = await owner.CreateGroupAsync(new CreateGroupRequest { Name = "Peña Historial", TournamentTypeId = 1, CreditsEnabled = true, InitialCredits = 50 });

            var history = await owner.GetCreditHistoryAsync(group.Id);

            var grant = Assert.Single(history);
            Assert.Equal("InitialGrant", grant.Reason);
            Assert.Equal(50, grant.Amount);
            Assert.Equal(50, grant.BalanceAfter);
        }

        [Fact]
        public async Task History_ShowsAdjustment_NewestFirst_ForMember()
        {
            var owner = await ClientAsync("jugador1@example.com", "Torneo2026");
            var group = await owner.CreateGroupAsync(new CreateGroupRequest { Name = "Peña Historial Ajuste", TournamentTypeId = 1, CreditsEnabled = true, InitialCredits = 50 });
            var member = await owner.InviteMemberAsync(group.Id, new InviteMemberRequest { Email = "jugador2@example.com" });
            await owner.AdjustCreditsAsync(group.Id, member.UserId, new AdjustCreditsRequest { Amount = 25 });

            var player = await ClientAsync("jugador2@example.com", "Torneo2026");
            var history = await player.GetCreditHistoryAsync(group.Id);

            Assert.Equal(2, history.Count);
            Assert.Equal("AdminAdjustment", history[0].Reason);
            Assert.Equal(25, history[0].Amount);
            Assert.Equal(75, history[0].BalanceAfter);
            Assert.Equal("InitialGrant", history[1].Reason);
        }

        [Fact]
        public async Task History_OnlyOwnTransactions()
        {
            var owner = await ClientAsync("jugador1@example.com", "Torneo2026");
            var group = await owner.CreateGroupAsync(new CreateGroupRequest { Name = "Peña Historial Aislado", TournamentTypeId = 1, CreditsEnabled = true, InitialCredits = 50 });
            var member = await owner.InviteMemberAsync(group.Id, new InviteMemberRequest { Email = "jugador2@example.com" });
            await owner.AdjustCreditsAsync(group.Id, member.UserId, new AdjustCreditsRequest { Amount = 25 });

            var ownerHistory = await owner.GetCreditHistoryAsync(group.Id);

            Assert.Single(ownerHistory);
            Assert.Equal("InitialGrant", ownerHistory[0].Reason);
        }

        [Fact]
        public async Task History_CreditsDisabled_Fails()
        {
            var owner = await ClientAsync("jugador1@example.com", "Torneo2026");
            var group = await owner.CreateGroupAsync(new CreateGroupRequest { Name = "Peña Sin Fichas Historial", TournamentTypeId = 1 });

            await Assert.ThrowsAsync<ApiException>(() => owner.GetCreditHistoryAsync(group.Id));
        }
    }
}
