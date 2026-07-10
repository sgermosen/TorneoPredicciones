using System.Collections.Generic;
using System.Threading.Tasks;
using CompeTournament.Backend.Helpers;
using CompeTournament.Mobile.Core.Services;
using CompeTournament.Shared.Auth;
using CompeTournament.Shared.Tournaments;
using Xunit;

namespace CompeTournament.Tests
{
    public class RecapTests : IClassFixture<ApiFactory>
    {
        private readonly ApiFactory _factory;

        public RecapTests(ApiFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task TemplateRecap_EmptyLeaderboard_InvitesToPredict()
        {
            var generator = new TemplateRecapGenerator();
            var text = await generator.GenerateAsync(new RecapContext
            {
                GroupName = "Copa Amistosa",
                Leaderboard = new List<LeaderboardEntryDto>()
            });

            Assert.Contains("Todavia no hay puntos", text);
        }

        [Fact]
        public async Task TemplateRecap_MentionsLeaderAndLastMatch()
        {
            var generator = new TemplateRecapGenerator();
            var text = await generator.GenerateAsync(new RecapContext
            {
                GroupName = "Copa Amistosa",
                Leaderboard = new List<LeaderboardEntryDto>
                {
                    new LeaderboardEntryDto { FullName = "Ana Lopez", Points = 9 },
                    new LeaderboardEntryDto { FullName = "Beto Cruz", Points = 8 }
                },
                LastMatchSummary = "TIG 2-1 LEO"
            });

            Assert.Contains("Ana Lopez", text);
            Assert.Contains("9 puntos", text);
            Assert.Contains("TIG 2-1 LEO", text);
        }

        [Fact]
        public async Task RecapEndpoint_ReturnsTextForGroup()
        {
            var store = new InMemoryTokenStore();
            var api = new ApiClient(_factory.CreateClient(), store);
            var token = await api.LoginAsync(new TokenRequest { Username = "sgrysoft@gmail.com", Password = "Torneo2026" });
            await store.SetTokensAsync(token.Token, token.RefreshToken);

            var recap = await api.GetRecapAsync(1);
            Assert.Equal(1, recap.GroupId);
            Assert.Contains("Starling Germosen", recap.Text);
        }
    }
}
