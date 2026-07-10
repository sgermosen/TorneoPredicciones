using System;
using System.Threading.Tasks;
using CompeTournament.Backend.Data;
using CompeTournament.Backend.Helpers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CompeTournament.Tests
{
    public class FakeResultsProvider : IMatchResultsProvider
    {
        public Task<MatchScore> TryGetResultAsync(int matchId) =>
            Task.FromResult(matchId == 1 ? new MatchScore { LocalPoints = 1, VisitorPoints = 0 } : null);
    }

    public class AutoCloseFactory : ApiFactory
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            base.ConfigureWebHost(builder);
            builder.ConfigureTestServices(services =>
            {
                services.AddScoped<IMatchResultsProvider, FakeResultsProvider>();
            });
        }
    }

    public class SchedulerTests : IClassFixture<ApiFactory>
    {
        private readonly ApiFactory _factory;

        public SchedulerTests(ApiFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task LockDueMatches_LocksMatchesPastKickoff()
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var scheduler = scope.ServiceProvider.GetRequiredService<IMatchScheduler>();

            var match = await db.Matches.FirstAsync(m => m.Id == 1);
            match.DateTime = DateTime.UtcNow.AddMinutes(-5);
            await db.SaveChangesAsync();

            var locked = await scheduler.LockDueMatchesAsync();
            Assert.True(locked >= 1);

            var reloaded = await db.Matches.AsNoTracking().FirstAsync(m => m.Id == 1);
            Assert.Equal(2, reloaded.StatusId);
        }

        [Fact]
        public async Task AutoClose_UsesResultsProvider_ToCloseLockedMatches()
        {
            await using var factory = new AutoCloseFactory();
            using var scope = factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var scheduler = scope.ServiceProvider.GetRequiredService<IMatchScheduler>();

            var match = await db.Matches.FirstAsync(m => m.Id == 1);
            match.DateTime = DateTime.UtcNow.AddMinutes(-5);
            await db.SaveChangesAsync();

            await scheduler.LockDueMatchesAsync();
            var closed = await scheduler.AutoCloseWithResultsAsync();
            Assert.True(closed >= 1);

            var reloaded = await db.Matches.AsNoTracking().FirstAsync(m => m.Id == 1);
            Assert.Equal(3, reloaded.StatusId);
            Assert.Equal(1, reloaded.LocalPoints);
        }
    }
}
