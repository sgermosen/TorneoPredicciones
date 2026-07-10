using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using CompeTournament.Mobile.Core.Services;
using CompeTournament.Shared.Auth;
using CompeTournament.Shared.Tournaments;
using Xunit;

namespace CompeTournament.Tests
{
    public class GamificationTests : IClassFixture<ApiFactory>
    {
        private readonly ApiFactory _factory;

        public GamificationTests(ApiFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task BankerExactPrediction_DoublesPoints_AndInsightsReflectStreak()
        {
            var http = _factory.CreateClient();
            var store = new InMemoryTokenStore();
            var api = new ApiClient(http, store);

            var login = await api.LoginAsync(new TokenRequest { Username = "sgrysoft@gmail.com", Password = "Torneo2026" });
            await store.SetTokensAsync(login.Token, login.RefreshToken);

            var saved = await api.SavePredictionAsync(new PredictionRequest
            {
                MatchId = 1,
                LocalPoints = 2,
                VisitorPoints = 1,
                IsBanker = true
            });
            Assert.True(saved.IsBanker);

            var close = new HttpRequestMessage(HttpMethod.Post, "api/matches/1/close")
            {
                Content = JsonContent.Create(new CloseMatchRequest { LocalPoints = 2, VisitorPoints = 1 })
            };
            close.Headers.Authorization = new AuthenticationHeaderValue("Bearer", login.Token);
            (await http.SendAsync(close)).EnsureSuccessStatusCode();

            var leaderboard = await api.GetLeaderboardAsync(1);
            var me = leaderboard.First(e => e.UserId == login.User.Id);
            Assert.Equal(6, me.Points);

            var insights = await api.GetInsightsAsync(1);
            Assert.Equal(1, insights.TotalResolved);
            Assert.Equal(1, insights.ExactHits);
            Assert.Equal(1, insights.CurrentStreak);
            Assert.Equal(6, insights.TotalPoints);
            Assert.Equal(1.0, insights.Accuracy);
        }

        [Fact]
        public async Task Banker_IsUniquePerGroup()
        {
            var http = _factory.CreateClient();
            var store = new InMemoryTokenStore();
            var api = new ApiClient(http, store);

            var login = await api.LoginAsync(new TokenRequest { Username = "sgrysoft@gmail.com", Password = "Torneo2026" });
            await store.SetTokensAsync(login.Token, login.RefreshToken);

            await api.SavePredictionAsync(new PredictionRequest { MatchId = 1, LocalPoints = 1, VisitorPoints = 0, IsBanker = true });
            await api.SavePredictionAsync(new PredictionRequest { MatchId = 2, LocalPoints = 0, VisitorPoints = 0, IsBanker = true });

            var mine = await api.GetMyPredictionsAsync(1);
            Assert.Single(mine.Where(p => p.IsBanker));
            Assert.Equal(2, mine.Single(p => p.IsBanker).MatchId);
        }
    }
}
