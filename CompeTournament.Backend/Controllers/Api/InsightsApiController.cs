namespace CompeTournament.Backend.Controllers.Api
{
    using CompeTournament.Backend.Data;
    using CompeTournament.Shared.Scoring;
    using CompeTournament.Shared.Tournaments;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using System.Linq;
    using System.Threading.Tasks;

    [Route("api/insights")]
    public class InsightsApiController : ApiControllerBase
    {
        private readonly ApplicationDbContext _context;

        public InsightsApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("me")]
        public async Task<ActionResult<InsightsDto>> Me([FromQuery] int? groupId)
        {
            var userId = CurrentUserId;

            var query = _context.Predictions
                .Where(p => p.CreatedBy == userId && p.Match.StatusId == 3);

            if (groupId.HasValue)
            {
                query = query.Where(p => p.Match.GroupId == groupId.Value);
            }

            var resolved = await query
                .OrderBy(p => p.Match.DateTime)
                .Select(p => new
                {
                    MatchLocal = p.Match.LocalPoints,
                    MatchVisitor = p.Match.VisitorPoints,
                    PredictionLocal = p.LocalPoints,
                    PredictionVisitor = p.VisitorPoints,
                    p.AdquiredPoints
                })
                .AsNoTracking()
                .ToListAsync();

            var insights = new InsightsDto { TotalResolved = resolved.Count };

            var currentStreak = 0;
            var bestStreak = 0;

            foreach (var item in resolved)
            {
                insights.TotalPoints += item.AdquiredPoints;

                var basePoints = PredictionScoring.CalculatePoints(
                    item.MatchLocal ?? 0, item.MatchVisitor ?? 0,
                    item.PredictionLocal ?? 0, item.PredictionVisitor ?? 0);

                if (basePoints == PredictionScoring.ExactPoints)
                {
                    insights.ExactHits++;
                }
                else if (basePoints == PredictionScoring.OutcomePoints)
                {
                    insights.OutcomeHits++;
                }
                else
                {
                    insights.Misses++;
                }

                if (basePoints > 0)
                {
                    currentStreak++;
                    bestStreak = currentStreak > bestStreak ? currentStreak : bestStreak;
                }
                else
                {
                    currentStreak = 0;
                }
            }

            insights.CurrentStreak = currentStreak;
            insights.BestStreak = bestStreak;
            insights.Accuracy = insights.TotalResolved == 0
                ? 0
                : (double)(insights.ExactHits + insights.OutcomeHits) / insights.TotalResolved;

            return Ok(insights);
        }
    }
}
