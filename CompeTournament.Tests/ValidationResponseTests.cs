using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using CompeTournament.Shared.Auth;
using Xunit;

namespace CompeTournament.Tests
{
    public class ValidationResponseTests : IClassFixture<ApiFactory>
    {
        private readonly ApiFactory _factory;

        public ValidationResponseTests(ApiFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task InvalidModel_Returns400_WithSpanishTitle_AndFieldErrors()
        {
            var client = _factory.CreateClient();

            var response = await client.PostAsJsonAsync("api/auth/register", new RegisterRequest());

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            var root = doc.RootElement;

            Assert.Equal("Hay uno o más errores de validación.", root.GetProperty("title").GetString());
            Assert.True(root.TryGetProperty("errors", out var errors) && errors.ValueKind == JsonValueKind.Object);
            Assert.True(errors.EnumerateObject().Any());
        }
    }
}
