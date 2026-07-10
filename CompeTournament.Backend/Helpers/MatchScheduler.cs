namespace CompeTournament.Backend.Helpers
{
    using CompeTournament.Backend.Data;
    using CompeTournament.Backend.Realtime;
    using Microsoft.AspNetCore.SignalR;
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class MatchScheduler : IMatchScheduler
    {
        private const int StatusOpen = 1;
        private const int StatusLocked = 2;

        private readonly ApplicationDbContext _context;
        private readonly IMatchClosingService _matchClosingService;
        private readonly IMatchResultsProvider _resultsProvider;
        private readonly IHubContext<TournamentHub> _hub;

        public MatchScheduler(
            ApplicationDbContext context,
            IMatchClosingService matchClosingService,
            IMatchResultsProvider resultsProvider,
            IHubContext<TournamentHub> hub)
        {
            _context = context;
            _matchClosingService = matchClosingService;
            _resultsProvider = resultsProvider;
            _hub = hub;
        }

        public async Task<int> LockDueMatchesAsync()
        {
            var now = DateTime.UtcNow;
            var due = await _context.Matches
                .Where(m => m.StatusId == StatusOpen && m.DateTime <= now)
                .ToListAsync();

            foreach (var match in due)
            {
                match.StatusId = StatusLocked;
                await _hub.Clients.Group(TournamentHub.GroupName(match.GroupId)).SendAsync("MatchLocked", match.Id);
            }

            if (due.Count > 0)
            {
                await _context.SaveChangesAsync();
            }

            return due.Count;
        }

        public async Task<int> AutoCloseWithResultsAsync()
        {
            var locked = await _context.Matches
                .Where(m => m.StatusId == StatusLocked)
                .Select(m => m.Id)
                .ToListAsync();

            var closed = 0;
            foreach (var matchId in locked)
            {
                var result = await _resultsProvider.TryGetResultAsync(matchId);
                if (result != null && await _matchClosingService.CloseMatchAsync(matchId, result.LocalPoints, result.VisitorPoints))
                {
                    closed++;
                }
            }

            return closed;
        }
    }
}
