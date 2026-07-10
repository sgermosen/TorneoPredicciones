namespace CompeTournament.Backend.Controllers.Api
{
    using CompeTournament.Backend.Data;
    using CompeTournament.Backend.Data.Entities;
    using CompeTournament.Shared.Tournaments;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    [Route("api/predictions")]
    public class PredictionsApiController : ApiControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PredictionsApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("mine")]
        public async Task<ActionResult<List<PredictionDto>>> Mine([FromQuery] int? groupId)
        {
            var userId = CurrentUserId;

            var query = _context.Predictions.Where(p => p.CreatedBy == userId);
            if (groupId.HasValue)
            {
                query = query.Where(p => p.Match.GroupId == groupId.Value);
            }

            var predictions = await query
                .AsNoTracking()
                .ToListAsync();

            return Ok(predictions.Select(p => new PredictionDto
            {
                Id = p.Id,
                MatchId = p.MatchId,
                LocalPoints = p.LocalPoints,
                VisitorPoints = p.VisitorPoints,
                IsBanker = p.IsBanker,
                AdquiredPoints = p.AdquiredPoints
            }).ToList());
        }

        [HttpPost]
        public async Task<ActionResult<PredictionDto>> Upsert([FromBody] PredictionRequest request)
        {
            var userId = CurrentUserId;

            var match = await _context.Matches.FirstOrDefaultAsync(m => m.Id == request.MatchId);
            if (match == null)
            {
                return NotFound(new { message = "El partido no existe." });
            }

            if (match.StatusId != 1)
            {
                return BadRequest(new { message = "El partido no admite predicciones." });
            }

            var isMember = await _context.GroupUsers
                .AnyAsync(gu => gu.GroupId == match.GroupId && gu.ApplicationUserId == userId && gu.IsAccepted && !gu.IsBlocked);
            if (!isMember)
            {
                return Forbid();
            }

            var prediction = await _context.Predictions
                .FirstOrDefaultAsync(p => p.MatchId == request.MatchId && p.CreatedBy == userId);

            if (prediction == null)
            {
                prediction = new Prediction
                {
                    MatchId = request.MatchId,
                    LocalPoints = request.LocalPoints,
                    VisitorPoints = request.VisitorPoints,
                    IsBanker = request.IsBanker,
                    AdquiredPoints = 0
                };
                _context.Predictions.Add(prediction);
            }
            else
            {
                prediction.LocalPoints = request.LocalPoints;
                prediction.VisitorPoints = request.VisitorPoints;
                prediction.IsBanker = request.IsBanker;
                _context.Predictions.Update(prediction);
            }

            if (request.IsBanker)
            {
                var others = await _context.Predictions
                    .Where(p => p.CreatedBy == userId && p.MatchId != request.MatchId && p.Match.GroupId == match.GroupId && p.IsBanker)
                    .ToListAsync();
                foreach (var other in others)
                {
                    other.IsBanker = false;
                }
            }

            await _context.SaveChangesAsync();

            return Ok(new PredictionDto
            {
                Id = prediction.Id,
                MatchId = prediction.MatchId,
                LocalPoints = prediction.LocalPoints,
                VisitorPoints = prediction.VisitorPoints,
                IsBanker = prediction.IsBanker,
                AdquiredPoints = prediction.AdquiredPoints
            });
        }
    }
}
