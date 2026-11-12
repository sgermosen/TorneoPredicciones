namespace CompeTournament.Backend.Controllers.Api
{
    using CompeTournament.Backend.Data;
    using CompeTournament.Backend.Helpers;
    using CompeTournament.Shared.Tournaments;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    [Route("api/matches")]
    public class MatchesApiController : ApiControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMatchClosingService _matchClosingService;

        public MatchesApiController(ApplicationDbContext context, IMatchClosingService matchClosingService)
        {
            _context = context;
            _matchClosingService = matchClosingService;
        }

        [HttpPost("{id:int}/close")]
        public async Task<IActionResult> Close(int id, [FromBody] CloseMatchRequest request)
        {
            var match = await _context.Matches
                .Include(m => m.Group)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);
            if (match == null)
            {
                return NotFound(new { message = "El partido no existe." });
            }

            if (!CanManageOwnedBy(match.Group?.CreatedBy))
            {
                return StatusCode(403, new { message = "No gestionas este torneo." });
            }

            var closed = await _matchClosingService.CloseMatchAsync(id, request.LocalPoints, request.VisitorPoints);
            if (!closed)
            {
                return Conflict(new { message = "El partido ya está cerrado." });
            }

            return NoContent();
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<MatchDto>> GetById(int id)
        {
            var userId = CurrentUserId;

            var match = await _context.Matches
                .Where(m => m.Id == id)
                .Include(m => m.Local)
                .Include(m => m.Visitor)
                .Include(m => m.Status)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (match == null)
            {
                return NotFound();
            }

            var prediction = await _context.Predictions
                .Where(p => p.MatchId == id && p.CreatedBy == userId)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            return Ok(new MatchDto
            {
                Id = match.Id,
                DateTime = DateTime.SpecifyKind(match.DateTime, DateTimeKind.Utc),
                GroupId = match.GroupId,
                LocalId = match.LocalId,
                LocalName = match.Local?.Name,
                LocalInitials = match.Local?.Initials,
                LocalPictureUrl = match.Local?.PictureUrl,
                VisitorId = match.VisitorId,
                VisitorName = match.Visitor?.Name,
                VisitorInitials = match.Visitor?.Initials,
                VisitorPictureUrl = match.Visitor?.PictureUrl,
                LocalPoints = match.LocalPoints,
                VisitorPoints = match.VisitorPoints,
                StatusId = match.StatusId,
                StatusName = match.Status?.Name,
                IsOpen = match.StatusId == 1,
                MyPrediction = prediction == null ? null : new PredictionDto
                {
                    Id = prediction.Id,
                    MatchId = prediction.MatchId,
                    LocalPoints = prediction.LocalPoints,
                    VisitorPoints = prediction.VisitorPoints,
                    IsBanker = prediction.IsBanker,
                    AdquiredPoints = prediction.AdquiredPoints
                }
            });
        }
    }
}
