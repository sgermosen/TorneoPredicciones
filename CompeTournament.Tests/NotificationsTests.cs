using System.Linq;
using System.Threading.Tasks;
using CompeTournament.Mobile.Core.Services;
using CompeTournament.Shared.Auth;
using Xunit;

namespace CompeTournament.Tests
{
    public class NotificationsTests : IClassFixture<ApiFactory>
    {
        private readonly ApiFactory _factory;

        public NotificationsTests(ApiFactory factory)
        {
            _factory = factory;
        }

        private async Task<ApiClient> ClientAsync(string email)
        {
            var store = new InMemoryTokenStore();
            var client = new ApiClient(_factory.CreateClient(), store);
            var token = await client.LoginAsync(new TokenRequest { Username = email, Password = "Torneo2026" });
            await store.SetTokensAsync(token.Token, token.RefreshToken);
            return client;
        }

        [Fact]
        public async Task Inbox_ReturnsSeededWelcome()
        {
            var client = await ClientAsync("jugador1@example.com");

            var notifications = await client.GetNotificationsAsync();

            Assert.Contains(notifications, n => n.Body.StartsWith("Bienvenido") && !n.IsRead);
        }

        [Fact]
        public async Task UnreadCount_IsPositive_ForSeededUser()
        {
            var client = await ClientAsync("jugador1@example.com");

            var count = await client.GetUnreadNotificationCountAsync();

            Assert.True(count >= 1);
        }

        [Fact]
        public async Task MarkRead_DecrementsUnread()
        {
            var client = await ClientAsync("jugador2@example.com");
            var before = await client.GetUnreadNotificationCountAsync();
            Assert.True(before >= 1);

            var notifications = await client.GetNotificationsAsync();
            var unread = notifications.First(n => !n.IsRead);
            await client.MarkNotificationReadAsync(unread.Id);

            var after = await client.GetUnreadNotificationCountAsync();
            Assert.Equal(before - 1, after);
        }

        [Fact]
        public async Task MarkAllRead_ZerosUnread()
        {
            var client = await ClientAsync("admin@example.com");
            Assert.True(await client.GetUnreadNotificationCountAsync() >= 1);

            await client.MarkAllNotificationsReadAsync();

            Assert.Equal(0, await client.GetUnreadNotificationCountAsync());
        }
    }
}
