using System;
using System.Linq;
using System.Threading.Tasks;
using CompeTournament.Mobile.Core.Services;
using CompeTournament.Shared.Auth;
using CompeTournament.Shared.Tournaments;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR.Client;
using Xunit;

namespace CompeTournament.Tests
{
    public class ChatTests : IClassFixture<ApiFactory>
    {
        private readonly ApiFactory _factory;

        public ChatTests(ApiFactory factory)
        {
            _factory = factory;
        }

        private async Task<(ApiClient api, TokenResponse token)> AuthAsync(string email, string password)
        {
            var store = new InMemoryTokenStore();
            var api = new ApiClient(_factory.CreateClient(), store);
            var token = await api.LoginAsync(new TokenRequest { Username = email, Password = password });
            await store.SetTokensAsync(token.Token, token.RefreshToken);
            return (api, token);
        }

        [Fact]
        public async Task Member_CanPostAndReadComments()
        {
            var (api, _) = await AuthAsync("sgrysoft@gmail.com", "Torneo2026");

            var posted = await api.PostCommentAsync(1, "Los Tigres ganan seguro!");
            Assert.Equal("Starling Germosen", posted.AuthorName);

            var comments = await api.GetCommentsAsync(1);
            Assert.Contains(comments, c => c.Body == "Los Tigres ganan seguro!");
        }

        [Fact]
        public async Task NonMember_CannotComment()
        {
            var client = _factory.CreateClient();
            var store = new InMemoryTokenStore();
            var api = new ApiClient(client, store);

            await api.RegisterAsync(new RegisterRequest
            {
                FirstName = "Silent",
                LastName = "User",
                Email = "silent@test.com",
                PhoneNumber = "8093333333",
                Password = "Silent2026",
                Confirm = "Silent2026"
            });
            var token = await api.LoginAsync(new TokenRequest { Username = "silent@test.com", Password = "Silent2026" });
            await store.SetTokensAsync(token.Token, token.RefreshToken);

            await Assert.ThrowsAsync<ApiException>(() => api.PostCommentAsync(1, "hola"));
        }

        [Fact]
        public async Task PostingComment_BroadcastsToGroupSubscribers()
        {
            var (api, token) = await AuthAsync("sgrysoft@gmail.com", "Torneo2026");

            var connection = new HubConnectionBuilder()
                .WithUrl(new Uri(_factory.Server.BaseAddress, $"hubs/tournament?access_token={token.Token}"), options =>
                {
                    options.HttpMessageHandlerFactory = _ => _factory.Server.CreateHandler();
                    options.Transports = HttpTransportType.LongPolling;
                })
                .Build();

            await using var live = new LiveTournamentClient(connection);
            var received = new TaskCompletionSource<CommentDto>(TaskCreationOptions.RunContinuationsAsynchronously);
            live.CommentPosted += payload => received.TrySetResult(payload);
            await live.JoinGroupAsync(1);

            await api.PostCommentAsync(1, "En vivo!");

            var completed = await Task.WhenAny(received.Task, Task.Delay(TimeSpan.FromSeconds(10)));
            Assert.Same(received.Task, completed);
            Assert.Equal("En vivo!", (await received.Task).Body);
        }
    }
}
