using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using CompeTournament.Shared.Auth;
using CompeTournament.Shared.Tournaments;
using Xunit;

namespace CompeTournament.Tests
{
    public class ApiIntegrationTests : IClassFixture<ApiFactory>
    {
        private readonly ApiFactory _factory;

        public ApiIntegrationTests(ApiFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task Groups_WithoutToken_ReturnsUnauthorized()
        {
            var client = _factory.CreateClient();
            var response = await client.GetAsync("/api/groups");
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Register_Then_Login_ReturnsToken()
        {
            var client = _factory.CreateClient();
            var email = "player1@test.com";

            var register = await client.PostAsJsonAsync("/api/auth/register", new RegisterRequest
            {
                FirstName = "Player",
                LastName = "One",
                Email = email,
                PhoneNumber = "8090000000",
                Password = "Player2026",
                Confirm = "Player2026"
            });
            Assert.Equal(HttpStatusCode.OK, register.StatusCode);

            var login = await client.PostAsJsonAsync("/api/auth/token", new TokenRequest
            {
                Username = email,
                Password = "Player2026"
            });
            Assert.Equal(HttpStatusCode.OK, login.StatusCode);

            var token = await login.Content.ReadFromJsonAsync<TokenResponse>();
            Assert.False(string.IsNullOrWhiteSpace(token.Token));
            Assert.Equal(email, token.User.Email);
        }

        [Fact]
        public async Task Login_WithWrongPassword_ReturnsUnauthorized()
        {
            var client = _factory.CreateClient();
            var login = await client.PostAsJsonAsync("/api/auth/token", new TokenRequest
            {
                Username = "sgrysoft@gmail.com",
                Password = "definitely-wrong"
            });
            Assert.Equal(HttpStatusCode.Unauthorized, login.StatusCode);
        }

        [Fact]
        public async Task Member_CanListGroups_GetDetail_AndPredict()
        {
            var client = _factory.CreateClient();
            var token = await LoginAsync(client, "sgrysoft@gmail.com", "Torneo2026");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var groups = await client.GetFromJsonAsync<List<GroupDto>>("/api/groups");
            Assert.NotEmpty(groups);
            var group = groups.First();
            Assert.True(group.IsMember);

            var detail = await client.GetFromJsonAsync<GroupDetailDto>($"/api/groups/{group.Id}");
            Assert.NotEmpty(detail.Matches);
            Assert.NotEmpty(detail.Standings);

            var openMatch = detail.Matches.First(m => m.IsOpen);

            var prediction = await client.PostAsJsonAsync("/api/predictions", new PredictionRequest
            {
                MatchId = openMatch.Id,
                LocalPoints = 2,
                VisitorPoints = 1
            });
            Assert.Equal(HttpStatusCode.OK, prediction.StatusCode);

            var saved = await prediction.Content.ReadFromJsonAsync<PredictionDto>();
            Assert.Equal(openMatch.Id, saved.MatchId);
            Assert.Equal(2, saved.LocalPoints);
            Assert.Equal(1, saved.VisitorPoints);

            var mine = await client.GetFromJsonAsync<List<PredictionDto>>("/api/predictions/mine");
            Assert.Contains(mine, p => p.MatchId == openMatch.Id);
        }

        [Fact]
        public async Task NonMember_CannotPredict()
        {
            var client = _factory.CreateClient();

            await client.PostAsJsonAsync("/api/auth/register", new RegisterRequest
            {
                FirstName = "Outsider",
                LastName = "User",
                Email = "outsider@test.com",
                PhoneNumber = "8091111111",
                Password = "Outsider2026",
                Confirm = "Outsider2026"
            });

            var token = await LoginAsync(client, "outsider@test.com", "Outsider2026");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var adminClient = _factory.CreateClient();
            var adminToken = await LoginAsync(adminClient, "sgrysoft@gmail.com", "Torneo2026");
            adminClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
            var detail = await adminClient.GetFromJsonAsync<GroupDetailDto>("/api/groups/1");
            var openMatch = detail.Matches.First(m => m.IsOpen);

            var prediction = await client.PostAsJsonAsync("/api/predictions", new PredictionRequest
            {
                MatchId = openMatch.Id,
                LocalPoints = 1,
                VisitorPoints = 0
            });
            Assert.Equal(HttpStatusCode.Forbidden, prediction.StatusCode);
        }

        private static async Task<string> LoginAsync(HttpClient client, string username, string password)
        {
            var login = await client.PostAsJsonAsync("/api/auth/token", new TokenRequest
            {
                Username = username,
                Password = password
            });
            login.EnsureSuccessStatusCode();
            var token = await login.Content.ReadFromJsonAsync<TokenResponse>();
            return token.Token;
        }
    }
}
