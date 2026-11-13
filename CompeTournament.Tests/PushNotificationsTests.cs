using System.Collections.Generic;
using System.Threading.Tasks;
using CompeTournament.Backend.Data;
using CompeTournament.Backend.Helpers;
using CompeTournament.Mobile.Core.Services;
using CompeTournament.Shared.Auth;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CompeTournament.Tests
{
    public class PushNotificationsTests : IClassFixture<ApiFactory>
    {
        private readonly ApiFactory _factory;

        public PushNotificationsTests(ApiFactory factory)
        {
            _factory = factory;
        }

        private sealed class RecordingPushSender : IPushSender
        {
            public List<string> Tokens { get; } = new List<string>();

            public Task SendAsync(IEnumerable<string> tokens, string message)
            {
                Tokens.AddRange(tokens);
                return Task.CompletedTask;
            }
        }

        [Fact]
        public async Task Notify_SendsPushToRegisteredDeviceToken()
        {
            var sender = new RecordingPushSender();
            var factory = _factory.WithWebHostBuilder(builder =>
                builder.ConfigureTestServices(services =>
                    services.AddSingleton<IPushSender>(sender)));

            var store = new InMemoryTokenStore();
            var api = new ApiClient(factory.CreateClient(), store);
            var token = await api.LoginAsync(new TokenRequest { Username = "jugador1@example.com", Password = "Torneo2026" });
            await store.SetTokensAsync(token.Token, token.RefreshToken);
            await api.RegisterDeviceAsync("push-token-xyz", "android");

            using var scope = factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var user = await db.Users.FirstAsync(u => u.Email == "jugador1@example.com");
            var notifier = scope.ServiceProvider.GetRequiredService<INotificationService>();

            await notifier.NotifyAsync(new[] { $"userId:{user.Id}" }, "Prueba push");

            Assert.Contains("push-token-xyz", sender.Tokens);
        }
    }
}
