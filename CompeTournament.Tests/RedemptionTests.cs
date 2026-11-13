using System.Threading.Tasks;
using CompeTournament.Mobile.Core.Services;
using CompeTournament.Shared.Auth;
using CompeTournament.Shared.Tournaments;
using Xunit;

namespace CompeTournament.Tests
{
    public class RedemptionTests : IClassFixture<ApiFactory>
    {
        private readonly ApiFactory _factory;

        public RedemptionTests(ApiFactory factory)
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
        public async Task Redeem_DebitsCredits_AndRecordsRequest()
        {
            var owner = await ClientAsync("jugador1@example.com", "Torneo2026");
            var group = await owner.CreateGroupAsync(new CreateGroupRequest { Name = "Peña Canje", TournamentTypeId = 1, CreditsEnabled = true, InitialCredits = 100 });
            var item = await owner.CreateRedemptionItemAsync(group.Id, new CreateRedemptionItemRequest { Name = "Camiseta", CostCredits = 60, Stock = 2 });

            var redemption = await owner.RedeemAsync(group.Id, new RedeemRequest { RedemptionItemId = item.Id, DeliveryInfo = "Calle 1" });
            Assert.Equal("Requested", redemption.Status);
            Assert.Equal(60, redemption.CostCredits);

            var detail = await owner.GetGroupAsync(group.Id);
            Assert.Equal(40, detail.MyCredits);

            var items = await owner.GetRedemptionItemsAsync(group.Id);
            Assert.Equal(1, items[0].Stock);

            var mine = await owner.GetMyRedemptionsAsync(group.Id);
            Assert.Single(mine);
        }

        [Fact]
        public async Task Redeem_InsufficientCredits_Fails()
        {
            var owner = await ClientAsync("jugador1@example.com", "Torneo2026");
            var group = await owner.CreateGroupAsync(new CreateGroupRequest { Name = "Peña Pobre", TournamentTypeId = 1, CreditsEnabled = true, InitialCredits = 10 });
            var item = await owner.CreateRedemptionItemAsync(group.Id, new CreateRedemptionItemRequest { Name = "Premio caro", CostCredits = 50 });

            await Assert.ThrowsAsync<ApiException>(() => owner.RedeemAsync(group.Id, new RedeemRequest { RedemptionItemId = item.Id }));
        }

        [Fact]
        public async Task CreateItem_RequiresCreditsEnabled()
        {
            var owner = await ClientAsync("jugador1@example.com", "Torneo2026");
            var group = await owner.CreateGroupAsync(new CreateGroupRequest { Name = "Peña Sin Fichas", TournamentTypeId = 1 });

            await Assert.ThrowsAsync<ApiException>(() => owner.CreateRedemptionItemAsync(group.Id, new CreateRedemptionItemRequest { Name = "Premio", CostCredits = 10 }));
        }

        [Fact]
        public async Task Reject_RefundsCredits_AndRestoresStock()
        {
            var owner = await ClientAsync("jugador1@example.com", "Torneo2026");
            var group = await owner.CreateGroupAsync(new CreateGroupRequest { Name = "Peña Reembolso", TournamentTypeId = 1, CreditsEnabled = true, InitialCredits = 100 });
            await owner.InviteMemberAsync(group.Id, new InviteMemberRequest { Email = "jugador2@example.com" });
            var item = await owner.CreateRedemptionItemAsync(group.Id, new CreateRedemptionItemRequest { Name = "Taza", CostCredits = 30, Stock = 1 });

            var redeemer = await ClientAsync("jugador2@example.com", "Torneo2026");
            var redemption = await redeemer.RedeemAsync(group.Id, new RedeemRequest { RedemptionItemId = item.Id });

            var afterRedeem = await redeemer.GetGroupAsync(group.Id);
            Assert.Equal(70, afterRedeem.MyCredits);

            await owner.RejectRedemptionAsync(group.Id, redemption.Id);

            var afterRefund = await redeemer.GetGroupAsync(group.Id);
            Assert.Equal(100, afterRefund.MyCredits);

            var items = await owner.GetRedemptionItemsAsync(group.Id);
            Assert.Equal(1, items[0].Stock);

            var all = await owner.GetGroupRedemptionsAsync(group.Id);
            Assert.Contains(all, r => r.Id == redemption.Id && r.Status == "Rejected");
        }

        [Fact]
        public async Task Deliver_MarksDelivered_WithoutRefund()
        {
            var owner = await ClientAsync("jugador1@example.com", "Torneo2026");
            var group = await owner.CreateGroupAsync(new CreateGroupRequest { Name = "Peña Entrega", TournamentTypeId = 1, CreditsEnabled = true, InitialCredits = 80 });
            var item = await owner.CreateRedemptionItemAsync(group.Id, new CreateRedemptionItemRequest { Name = "Código digital", CostCredits = 20, IsDigital = true });

            var redemption = await owner.RedeemAsync(group.Id, new RedeemRequest { RedemptionItemId = item.Id });
            await owner.DeliverRedemptionAsync(group.Id, redemption.Id, new DeliverRedemptionRequest { DeliveryReference = "ABC-123" });

            var mine = await owner.GetMyRedemptionsAsync(group.Id);
            Assert.Contains(mine, r => r.Id == redemption.Id && r.Status == "Delivered" && r.DeliveryReference == "ABC-123");

            var detail = await owner.GetGroupAsync(group.Id);
            Assert.Equal(60, detail.MyCredits);
        }

        [Fact]
        public async Task Cancel_Own_RefundsCredits_AndRestoresStock()
        {
            var owner = await ClientAsync("jugador1@example.com", "Torneo2026");
            var group = await owner.CreateGroupAsync(new CreateGroupRequest { Name = "Peña Cancela", TournamentTypeId = 1, CreditsEnabled = true, InitialCredits = 70 });
            var item = await owner.CreateRedemptionItemAsync(group.Id, new CreateRedemptionItemRequest { Name = "Gorra", CostCredits = 25, Stock = 3 });

            var redemption = await owner.RedeemAsync(group.Id, new RedeemRequest { RedemptionItemId = item.Id });
            await owner.CancelRedemptionAsync(group.Id, redemption.Id);

            var detail = await owner.GetGroupAsync(group.Id);
            Assert.Equal(70, detail.MyCredits);

            var items = await owner.GetRedemptionItemsAsync(group.Id);
            Assert.Equal(3, items[0].Stock);
        }

        [Fact]
        public async Task NonOwner_CannotCreateItem()
        {
            var owner = await ClientAsync("jugador1@example.com", "Torneo2026");
            var group = await owner.CreateGroupAsync(new CreateGroupRequest { Name = "Peña Privada", TournamentTypeId = 1, CreditsEnabled = true, InitialCredits = 50 });
            await owner.InviteMemberAsync(group.Id, new InviteMemberRequest { Email = "jugador2@example.com" });

            var member = await ClientAsync("jugador2@example.com", "Torneo2026");
            await Assert.ThrowsAsync<ApiException>(() => member.CreateRedemptionItemAsync(group.Id, new CreateRedemptionItemRequest { Name = "Intruso", CostCredits = 10 }));
        }
    }
}
