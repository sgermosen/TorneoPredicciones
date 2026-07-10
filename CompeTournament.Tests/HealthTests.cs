using System.Threading.Tasks;
using Xunit;

namespace CompeTournament.Tests
{
    public class HealthTests : IClassFixture<ApiFactory>
    {
        private readonly ApiFactory _factory;

        public HealthTests(ApiFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task Health_ReportsHealthy()
        {
            var client = _factory.CreateClient();
            var response = await client.GetAsync("/health");

            Assert.True(response.IsSuccessStatusCode);
            var body = await response.Content.ReadAsStringAsync();
            Assert.Equal("Healthy", body);
        }
    }
}
