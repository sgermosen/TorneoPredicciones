namespace CompeTournament.Backend.Helpers
{
    using CompeTournament.Backend.Data;
    using CompeTournament.Backend.Realtime;
    using CompeTournament.Shared.Live;
    using CompeTournament.Shared.Scoring;
    using CompeTournament.Shared.Tournaments;
    using Microsoft.AspNetCore.SignalR;
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class MatchClosingService : IMatchClosingService
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationService _notificationService;
        private readonly IHubContext<TournamentHub> _hub;

        public MatchClosingService(ApplicationDbContext context, INotificationService notificationService, IHubContext<TournamentHub> hub)
        {
            _context = context;
            _notificationService = notificationService;
            _hub = hub;
        }

        public async Task<bool> CloseMatchAsync(int matchId, int localPoints, int visitorPoints)
        {
            var match = await _context.Matches.FirstOrDefaultAsync(m => m.Id == matchId);
            if (match == null || match.StatusId == 3)
            {
                return false;
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            match.LocalPoints = localPoints;
            match.VisitorPoints = visitorPoints;
            match.StatusId = 3;
            _context.Entry(match).State = EntityState.Modified;

            var outcome = PredictionScoring.GetOutcome(localPoints, visitorPoints);

            var local = await _context.Teams.FirstOrDefaultAsync(t => t.League.GroupId == match.GroupId && t.Id == match.LocalId);
            var visitor = await _context.Teams.FirstOrDefaultAsync(t => t.League.GroupId == match.GroupId && t.Id == match.VisitorId);

            if (local != null && visitor != null)
            {
                local.MatchesPlayed = (local.MatchesPlayed ?? 0) + 1;
                local.FavorPoints = (local.FavorPoints ?? 0) + localPoints;
                local.AgainstPoints = (local.AgainstPoints ?? 0) + visitorPoints;

                visitor.MatchesPlayed = (visitor.MatchesPlayed ?? 0) + 1;
                visitor.FavorPoints = (visitor.FavorPoints ?? 0) + visitorPoints;
                visitor.AgainstPoints = (visitor.AgainstPoints ?? 0) + localPoints;

                if (outcome == MatchOutcome.LocalWin)
                {
                    local.MatchesWon = (local.MatchesWon ?? 0) + 1;
                    local.CumulativePoints = (local.CumulativePoints ?? 0) + 3;
                    visitor.MatchesLost = (visitor.MatchesLost ?? 0) + 1;
                }
                else if (outcome == MatchOutcome.VisitorWin)
                {
                    visitor.MatchesWon = (visitor.MatchesWon ?? 0) + 1;
                    visitor.CumulativePoints = (visitor.CumulativePoints ?? 0) + 3;
                    local.MatchesLost = (local.MatchesLost ?? 0) + 1;
                }
                else
                {
                    local.MatchesTied = (local.MatchesTied ?? 0) + 1;
                    visitor.MatchesTied = (visitor.MatchesTied ?? 0) + 1;
                    local.CumulativePoints = (local.CumulativePoints ?? 0) + 1;
                    visitor.CumulativePoints = (visitor.CumulativePoints ?? 0) + 1;
                }

                _context.Entry(local).State = EntityState.Modified;
                _context.Entry(visitor).State = EntityState.Modified;
                await _context.SaveChangesAsync();
            }

            var teams = await _context.Teams.Where(t => t.League.GroupId == match.GroupId).ToListAsync();
            var position = 1;
            foreach (var team in teams
                .OrderByDescending(t => t.CumulativePoints)
                .ThenByDescending(t => (t.FavorPoints ?? 0) - (t.AgainstPoints ?? 0))
                .ThenByDescending(t => t.FavorPoints))
            {
                team.Position = position;
                _context.Entry(team).State = EntityState.Modified;
                position++;
            }

            var noPoints = new List<string>();
            var onePoint = new List<string>();
            var threePoints = new List<string>();

            var predictions = await _context.Predictions.Where(p => p.MatchId == match.Id).ToListAsync();
            foreach (var prediction in predictions)
            {
                var basePoints = PredictionScoring.CalculatePoints(
                    localPoints, visitorPoints,
                    prediction.LocalPoints ?? 0, prediction.VisitorPoints ?? 0);

                if (basePoints == PredictionScoring.ExactPoints)
                {
                    threePoints.Add($"userId:{prediction.CreatedBy}");
                }
                else if (basePoints == PredictionScoring.OutcomePoints)
                {
                    onePoint.Add($"userId:{prediction.CreatedBy}");
                }
                else
                {
                    noPoints.Add($"userId:{prediction.CreatedBy}");
                }

                var points = prediction.IsBanker ? basePoints * 2 : basePoints;

                if (points != 0)
                {
                    prediction.AdquiredPoints = points;
                    _context.Entry(prediction).State = EntityState.Modified;

                    var userGroup = await _context.GroupUsers
                        .FirstOrDefaultAsync(gu => gu.GroupId == match.GroupId
                            && gu.ApplicationUserId == prediction.CreatedBy
                            && gu.IsAccepted && !gu.IsBlocked);

                    if (userGroup != null)
                    {
                        userGroup.Points += points;
                        _context.Entry(userGroup).State = EntityState.Modified;
                    }

                    await _context.SaveChangesAsync();
                }
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            await NotifyAsync(threePoints, "Has ganado 3 puntos, felicidades!");
            await NotifyAsync(onePoint, "Has ganado 1 punto, felicidades!");
            await NotifyAsync(noPoints, "El partido termino, esta vez no sumaste puntos.");

            await BroadcastAsync(match.GroupId, match.Id, localPoints, visitorPoints);
            return true;
        }

        private async Task NotifyAsync(List<string> tags, string message)
        {
            if (tags.Count > 0)
            {
                await _notificationService.NotifyAsync(tags, message);
            }
        }

        private async Task BroadcastAsync(int groupId, int matchId, int localPoints, int visitorPoints)
        {
            var standings = await _context.Teams
                .Where(t => t.League.GroupId == groupId)
                .OrderBy(t => t.Position)
                .Select(t => new StandingDto
                {
                    TeamId = t.Id,
                    Position = t.Position,
                    Name = t.Name,
                    Initials = t.Initials,
                    PictureUrl = t.PictureUrl,
                    MatchesPlayed = t.MatchesPlayed,
                    MatchesWon = t.MatchesWon,
                    MatchesTied = t.MatchesTied,
                    MatchesLost = t.MatchesLost,
                    FavorPoints = t.FavorPoints,
                    AgainstPoints = t.AgainstPoints,
                    CumulativePoints = t.CumulativePoints
                })
                .ToListAsync();

            var leaderboard = await _context.GroupUsers
                .Where(gu => gu.GroupId == groupId && gu.IsAccepted && !gu.IsBlocked)
                .Include(gu => gu.ApplicationUser)
                .OrderByDescending(gu => gu.Points)
                .Select(gu => new LeaderboardEntryDto
                {
                    UserId = gu.ApplicationUserId,
                    FullName = gu.ApplicationUser != null ? gu.ApplicationUser.FullName : null,
                    Points = gu.Points,
                    IsAccepted = gu.IsAccepted,
                    IsBlocked = gu.IsBlocked
                })
                .ToListAsync();

            var payload = new LiveMatchClosedDto
            {
                GroupId = groupId,
                MatchId = matchId,
                LocalPoints = localPoints,
                VisitorPoints = visitorPoints,
                Standings = standings,
                Leaderboard = leaderboard
            };

            await _hub.Clients.Group(TournamentHub.GroupName(groupId)).SendAsync("MatchClosed", payload);
        }
    }
}
