using System.Linq;
using System.Threading.Tasks;
using CompeTournament.Backend.Data;
using CompeTournament.Mobile.Core.Services;
using CompeTournament.Shared.Auth;
using CompeTournament.Shared.Tournaments;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CompeTournament.Tests
{
    public class TournamentPointsTests : IClassFixture<ApiFactory>
    {
        private readonly ApiFactory _factory;

        public TournamentPointsTests(ApiFactory factory)
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
        public async Task GroupDto_ExposesMyPointsPerGroup()
        {
            var owner = await ClientAsync("jugador1@example.com", "Torneo2026");
            var group = await owner.CreateGroupAsync(new CreateGroupRequest { Name = "Peña Puntos", TournamentTypeId = 1 });

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var membership = context.GroupUsers.First(gu => gu.GroupId == group.Id);
                membership.Points = 7;
                context.SaveChanges();
            }

            var mine = await owner.GetMyGroupsAsync();
            var g = mine.Single(x => x.Id == group.Id);

            Assert.Equal(7, g.MyPoints);
        }
    }
}
