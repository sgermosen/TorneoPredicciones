using System.Threading.Tasks;
using CompeTournament.Backend.Data;
using CompeTournament.Mobile.Core.Services;
using CompeTournament.Shared.Auth;
using CompeTournament.Shared.Tournaments;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CompeTournament.Tests
{
    public class GroupCreationTests : IClassFixture<ApiFactory>
    {
        private readonly ApiFactory _factory;

        public GroupCreationTests(ApiFactory factory)
        {
            _factory = factory;
        }

        private async Task<ApiClient> AuthenticatedClientAsync(string email, string password)
        {
            var store = new InMemoryTokenStore();
            var client = new ApiClient(_factory.CreateClient(), store);
            var token = await client.LoginAsync(new TokenRequest { Username = email, Password = password });
            await store.SetTokensAsync(token.Token, token.RefreshToken);
            return client;
        }

        private async Task<(ApiClient Api, UserDto Me)> NewUserClientAsync(string email, string password)
        {
            var store = new InMemoryTokenStore();
            var client = new ApiClient(_factory.CreateClient(), store);
            await client.RegisterAsync(new RegisterRequest
            {
                FirstName = "Nuevo",
                LastName = "Dueño",
                Email = email,
                PhoneNumber = "8090000000",
                Password = password,
                Confirm = password
            });
            var token = await client.LoginAsync(new TokenRequest { Username = email, Password = password });
            await store.SetTokensAsync(token.Token, token.RefreshToken);
            var me = await client.GetMeAsync();
            return (client, me);
        }

        [Fact]
        public async Task AnyUser_CanCreateGroup_AndBecomesAcceptedMember()
        {
            var (api, _) = await NewUserClientAsync("dueno1@test.com", "Dueno2026");

            var group = await api.CreateGroupAsync(new CreateGroupRequest
            {
                Name = "Mi Peña",
                Requirements = "Solo amigos",
                TournamentTypeId = 1
            });

            Assert.True(group.Id > 0);
            Assert.Equal("Mi Peña", group.Name);
            Assert.True(group.IsMember);
            Assert.True(group.IsAccepted);
            Assert.Equal(1, group.MembersCount);

            var mine = await api.GetMyGroupsAsync();
            Assert.Contains(mine, g => g.Id == group.Id);
        }

        [Fact]
        public async Task CreatedGroup_RecordsCreatorAsOwner()
        {
            var (api, me) = await NewUserClientAsync("dueno2@test.com", "Dueno2026");

            var group = await api.CreateGroupAsync(new CreateGroupRequest
            {
                Name = "Peña Dueño",
                TournamentTypeId = 1
            });

            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var saved = await context.Groups.AsNoTracking().FirstAsync(g => g.Id == group.Id);

            Assert.False(string.IsNullOrWhiteSpace(saved.CreatedBy));
            Assert.Equal(me.Id, saved.CreatedBy);
        }

        [Fact]
        public async Task CreateGroup_InvalidTournamentType_Throws()
        {
            var api = await AuthenticatedClientAsync("jugador1@example.com", "Torneo2026");

            await Assert.ThrowsAsync<ApiException>(() => api.CreateGroupAsync(new CreateGroupRequest
            {
                Name = "Torneo sin tipo",
                TournamentTypeId = 9999
            }));
        }

        [Fact]
        public async Task CreateGroup_EmptyName_Throws()
        {
            var api = await AuthenticatedClientAsync("jugador1@example.com", "Torneo2026");

            await Assert.ThrowsAsync<ApiException>(() => api.CreateGroupAsync(new CreateGroupRequest
            {
                Name = "",
                TournamentTypeId = 1
            }));
        }
    }
}
