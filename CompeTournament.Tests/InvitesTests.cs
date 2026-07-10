using System.Linq;
using System.Threading.Tasks;
using CompeTournament.Mobile.Core.Services;
using CompeTournament.Shared.Auth;
using Xunit;

namespace CompeTournament.Tests
{
    public class InvitesTests : IClassFixture<ApiFactory>
    {
        private readonly ApiFactory _factory;

        public InvitesTests(ApiFactory factory)
        {
            _factory = factory;
        }

        private async Task<ApiClient> AuthenticatedClientAsync(string email, string password)
        {
            var store = new InMemoryTokenStore();
            var client = new ApiClient(_factory.CreateClient(), store);
            var token = await client.LoginAsync(new TokenRequest { Username = email, Password = password });
            await store.SetTokensAsync(token.Token, token.RefreshToken);
            return client;
        }

        [Fact]
        public async Task Member_CanReadInviteCode()
        {
            var client = await AuthenticatedClientAsync("sgrysoft@gmail.com", "Torneo2026");
            var invite = await client.GetInviteAsync(1);
            Assert.Equal("COPA2026", invite.Code);
        }

        [Fact]
        public async Task JoinByCode_AddsUserAsAcceptedMember()
        {
            var client = _factory.CreateClient();
            var store = new InMemoryTokenStore();
            var api = new ApiClient(client, store);

            await api.RegisterAsync(new RegisterRequest
            {
                FirstName = "Invited",
                LastName = "Player",
                Email = "invited@test.com",
                PhoneNumber = "8092222222",
                Password = "Invited2026",
                Confirm = "Invited2026"
            });

            var token = await api.LoginAsync(new TokenRequest { Username = "invited@test.com", Password = "Invited2026" });
            await store.SetTokensAsync(token.Token, token.RefreshToken);

            var group = await api.JoinByCodeAsync("COPA2026");
            Assert.True(group.IsMember);
            Assert.True(group.IsAccepted);

            var mine = await api.GetMyGroupsAsync();
            Assert.Contains(mine, g => g.Id == group.Id);
        }

        [Fact]
        public async Task JoinByCode_InvalidCode_Throws()
        {
            var api = await AuthenticatedClientAsync("elis@gmail.com", "Torneo2026");
            await Assert.ThrowsAsync<ApiException>(() => api.JoinByCodeAsync("NOPE9999"));
        }
    }
}
