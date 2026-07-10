using System;
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
    public class LiveTournamentTests : IClassFixture<ApiFactory>
    {
        private readonly ApiFactory _factory;

        public LiveTournamentTests(ApiFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task ClosingMatch_BroadcastsLiveUpdate_ToGroupSubscribers()
        {
            var http = _factory.CreateClient();
            var store = new InMemoryTokenStore();
            var apiClient = new ApiClient(http, store);

            var login = await apiClient.LoginAsync(new TokenRequest
            {
                Username = "sgrysoft@gmail.com",
                Password = "Torneo2026"
            });
            await store.SetTokensAsync(login.Token, login.RefreshToken);

            var connection = new HubConnectionBuilder()
                .WithUrl(new Uri(_factory.Server.BaseAddress, $"hubs/tournament?access_token={login.Token}"), options =>
                {
                    options.HttpMessageHandlerFactory = _ => _factory.Server.CreateHandler();
                    options.Transports = HttpTransportType.LongPolling;
                })
                .Build();

            await using var live = new LiveTournamentClient(connection);

            var received = new TaskCompletionSource<LiveMatchClosedDto>(TaskCreationOptions.RunContinuationsAsynchronously);
            live.MatchClosed += payload => received.TrySetResult(payload);

            await live.JoinGroupAsync(1);

            var closeRequest = new HttpRequestMessage(HttpMethod.Post, "api/matches/1/close")
            {
                Content = JsonContent.Create(new CloseMatchRequest { LocalPoints = 2, VisitorPoints = 1 })
            };
            closeRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", login.Token);
            var closeResponse = await http.SendAsync(closeRequest);
            closeResponse.EnsureSuccessStatusCode();

            var completed = await Task.WhenAny(received.Task, Task.Delay(TimeSpan.FromSeconds(10)));
            Assert.Same(received.Task, completed);

            var payload = await received.Task;
            Assert.Equal(1, payload.GroupId);
            Assert.Equal(1, payload.MatchId);
            Assert.Equal(2, payload.LocalPoints);
            Assert.NotEmpty(payload.Standings);
        }
    }
}
