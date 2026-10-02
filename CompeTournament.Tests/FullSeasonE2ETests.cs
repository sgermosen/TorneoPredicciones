using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using CompeTournament.Mobile.Core.Services;
using CompeTournament.Shared.Auth;
using CompeTournament.Shared.Live;
using CompeTournament.Shared.Tournaments;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR.Client;
using Xunit;

namespace CompeTournament.Tests
{
    public class FullSeasonE2ETests : IClassFixture<ApiFactory>
    {
        private readonly ApiFactory _factory;

        public FullSeasonE2ETests(ApiFactory factory)
        {
            _factory = factory;
        }

        private async Task<(ApiClient api, TokenResponse token)> RegisterJoinAsync(string first, string email)
        {
            var store = new InMemoryTokenStore();
            var api = new ApiClient(_factory.CreateClient(), store);
            await api.RegisterAsync(new RegisterRequest
            {
                FirstName = first,
                LastName = "Test",
                Email = email,
                PhoneNumber = "8090000000",
                Password = "Player2026",
                Confirm = "Player2026"
            });
            var token = await api.LoginAsync(new TokenRequest { Username = email, Password = "Player2026" });
            await store.SetTokensAsync(token.Token, token.RefreshToken);
            await api.JoinByCodeAsync("COPA2026");
            return (api, token);
        }

        private async Task<(ApiClient api, TokenResponse token, HttpClient http)> AdminAsync()
        {
            var store = new InMemoryTokenStore();
            var http = _factory.CreateClient();
            var api = new ApiClient(http, store);
            var token = await api.LoginAsync(new TokenRequest { Username = "sgrysoft@gmail.com", Password = "Torneo2026" });
            await store.SetTokensAsync(token.Token, token.RefreshToken);
            return (api, token, http);
        }

        private static async Task CloseMatchAsync(HttpClient http, string bearer, int matchId, int local, int visitor)
        {
            var req = new HttpRequestMessage(HttpMethod.Post, $"api/matches/{matchId}/close")
            {
                Content = JsonContent.Create(new CloseMatchRequest { LocalPoints = local, VisitorPoints = visitor })
            };
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearer);
            (await http.SendAsync(req)).EnsureSuccessStatusCode();
        }

        [Fact]
        public async Task FullSeason_MultipleUsers_ScoresRankingInsightsChatRecapAndLive()
        {
            // health
            Assert.True((await _factory.CreateClient().GetAsync("/health")).IsSuccessStatusCode);

            // 3 jugadores se registran y se unen por codigo
            var (ana, anaTok) = await RegisterJoinAsync("Ana", "ana.e2e@test.com");
            var (beto, _) = await RegisterJoinAsync("Beto", "beto.e2e@test.com");
            var (caro, _) = await RegisterJoinAsync("Caro", "caro.e2e@test.com");

            // el grupo demo tiene 2 partidos abiertos (id 1: TIG-LEO, id 2: AGU-TOR)
            var detail = await ana.GetGroupAsync(1);
            var open = detail.Matches.Where(m => m.IsOpen).OrderBy(m => m.Id).ToList();
            Assert.True(open.Count >= 2);
            int m1 = open[0].Id, m2 = open[1].Id;

            // Ana: exacto en ambos; banker en m1  -> 6 + 3 = 9
            await ana.SavePredictionAsync(new PredictionRequest { MatchId = m1, LocalPoints = 2, VisitorPoints = 1, IsBanker = true });
            await ana.SavePredictionAsync(new PredictionRequest { MatchId = m2, LocalPoints = 0, VisitorPoints = 0 });

            // Beto: acierta solo el resultado en ambos -> 1 + 1 = 2
            await beto.SavePredictionAsync(new PredictionRequest { MatchId = m1, LocalPoints = 3, VisitorPoints = 0 });
            await beto.SavePredictionAsync(new PredictionRequest { MatchId = m2, LocalPoints = 1, VisitorPoints = 1 });

            // Caro: falla ambos -> 0
            await caro.SavePredictionAsync(new PredictionRequest { MatchId = m1, LocalPoints = 0, VisitorPoints = 2 });
            await caro.SavePredictionAsync(new PredictionRequest { MatchId = m2, LocalPoints = 2, VisitorPoints = 0 });

            // chat antes de cerrar
            await ana.PostCommentAsync(m1, "Banker puesto!");
            Assert.Contains(await ana.GetCommentsAsync(m1), c => c.Body == "Banker puesto!");

            // admin: suscripcion en vivo + cierre de partidos
            var (_, adminTok, http) = await AdminAsync();

            var connection = new HubConnectionBuilder()
                .WithUrl(new Uri(_factory.Server.BaseAddress, $"hubs/tournament?access_token={anaTok.Token}"), o =>
                {
                    o.HttpMessageHandlerFactory = _ => _factory.Server.CreateHandler();
                    o.Transports = HttpTransportType.LongPolling;
                }).Build();
            await using var live = new LiveTournamentClient(connection);
            var liveUpdate = new TaskCompletionSource<LiveMatchClosedDto>(TaskCreationOptions.RunContinuationsAsynchronously);
            live.MatchClosed += p => liveUpdate.TrySetResult(p);
            await live.JoinGroupAsync(1);

            await CloseMatchAsync(http, adminTok.Token, m1, 2, 1); // local win, exacto para Ana
            await CloseMatchAsync(http, adminTok.Token, m2, 0, 0); // empate, exacto para Ana

            // el broadcast en vivo llego con Ana liderando
            var completed = await Task.WhenAny(liveUpdate.Task, Task.Delay(TimeSpan.FromSeconds(10)));
            Assert.Same(liveUpdate.Task, completed);
            Assert.Equal(1, (await liveUpdate.Task).GroupId);

            // leaderboard final: Ana(9) > Beto(2) > Caro(0)
            var board = await ana.GetLeaderboardAsync(1);
            int PointsOf(string id) => board.First(e => e.UserId == id).Points;
            Assert.Equal(9, PointsOf(anaTok.User.Id));
            var ordered = board.Where(e => e.Points > 0).Select(e => e.Points).ToList();
            Assert.Equal(ordered.OrderByDescending(x => x).ToList(), ordered); // viene ordenado desc
            Assert.Equal(9, board.Max(e => e.Points));

            // insights de Ana: 2 resueltos, 2 exactos, racha 2, 9 puntos
            var anaInsights = await ana.GetInsightsAsync(1);
            Assert.Equal(2, anaInsights.TotalResolved);
            Assert.Equal(2, anaInsights.ExactHits);
            Assert.Equal(2, anaInsights.CurrentStreak);
            Assert.Equal(9, anaInsights.TotalPoints);
            Assert.Equal(1.0, anaInsights.Accuracy);

            // insights de Caro: 2 resueltos, 2 fallos, racha 0
            var caroInsights = await caro.GetInsightsAsync(1);
            Assert.Equal(2, caroInsights.TotalResolved);
            Assert.Equal(2, caroInsights.Misses);
            Assert.Equal(0, caroInsights.CurrentStreak);
            Assert.Equal(0, caroInsights.TotalPoints);

            // recap menciona al lider
            var recap = await ana.GetRecapAsync(1);
            Assert.Contains("Ana", recap.Text);

            // registro de dispositivo
            await ana.RegisterDeviceAsync("device-e2e-1", "android");
            Assert.Contains("device-e2e-1", await ana.GetMyDevicesAsync());

            // los partidos ya no admiten prediccion (cerrados)
            await Assert.ThrowsAsync<ApiException>(() =>
                ana.SavePredictionAsync(new PredictionRequest { MatchId = m1, LocalPoints = 1, VisitorPoints = 1 }));
        }
    }
}
