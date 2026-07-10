using System.Threading.Tasks;
using CompeTournament.Mobile.Core.Services;
using CompeTournament.Shared.Auth;
using Xunit;

namespace CompeTournament.Tests
{
    public class DevicesTests : IClassFixture<ApiFactory>
    {
        private readonly ApiFactory _factory;

        public DevicesTests(ApiFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task Device_CanBeRegisteredAndUnregistered()
        {
            var store = new InMemoryTokenStore();
            var api = new ApiClient(_factory.CreateClient(), store);
            var token = await api.LoginAsync(new TokenRequest { Username = "elis@gmail.com", Password = "Torneo2026" });
            await store.SetTokensAsync(token.Token, token.RefreshToken);

            await api.RegisterDeviceAsync("device-token-abc", "android");
            var devices = await api.GetMyDevicesAsync();
            Assert.Contains("device-token-abc", devices);

            await api.UnregisterDeviceAsync("device-token-abc");
            var afterRemoval = await api.GetMyDevicesAsync();
            Assert.DoesNotContain("device-token-abc", afterRemoval);
        }
    }
}
