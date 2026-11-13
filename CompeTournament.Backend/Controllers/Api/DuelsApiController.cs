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

    [Route("api/groups/{groupId:int}")]
    public class DuelsApiController : ApiControllerBase
    {
        private const int StatusTerminado = 3;

        private readonly ApplicationDbContext _context;

        public DuelsApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("duels")]
        public async Task<ActionResult<DuelDto>> Create(int groupId, [FromBody] CreateDuelRequest request)
        {
            var userId = CurrentUserId;

            var group = await _context.Groups.AsNoTracking().FirstOrDefaultAsync(g => g.Id == groupId);
            if (group == null)
            {
                return NotFound();
            }

            if (string.IsNullOrWhiteSpace(request.OpponentUserId) || request.OpponentUserId == userId)
            {
                return BadRequest(new { message = "Elige a otro miembro para el duelo." });
            }

            var members = await _context.GroupUsers.AsNoTracking()
                .Where(gu => gu.GroupId == groupId && gu.IsAccepted && !gu.IsBlocked)
                .Select(gu => gu.ApplicationUserId)
                .ToListAsync();

            if (!members.Contains(userId) || !members.Contains(request.OpponentUserId))
            {
                return BadRequest(new { message = "Ambos deben ser miembros aprobados del torneo." });
            }

            var match = await _context.Matches.AsNoTracking().FirstOrDefaultAsync(m => m.Id == request.MatchId && m.GroupId == groupId);
            if (match == null)
            {
                return NotFound(new { message = "El partido no es de este torneo." });
            }

            var exists = await _context.Duels.AsNoTracking().AnyAsync(d => d.MatchId == request.MatchId
                && d.Status != "Declined"
                && ((d.ChallengerUserId == userId && d.OpponentUserId == request.OpponentUserId)
                    || (d.ChallengerUserId == request.OpponentUserId && d.OpponentUserId == userId)));
            if (exists)
            {
                return Conflict(new { message = "Ya hay un duelo con esa persona en este partido." });
            }

            var duel = new Duel
            {
                GroupId = groupId,
                MatchId = request.MatchId,
                ChallengerUserId = userId,
                OpponentUserId = request.OpponentUserId,
                Status = "Pending"
            };
            _context.Duels.Add(duel);
            await _context.SaveChangesAsync();

            var list = await BuildDuelsAsync(groupId, userId, new List<int> { duel.Id });
            return Ok(list.First());
        }

        [HttpPost("duels/{id:int}/accept")]
        public async Task<IActionResult> Accept(int groupId, int id)
        {
            return await RespondAsync(groupId, id, "Accepted");
        }

        [HttpPost("duels/{id:int}/decline")]
        public async Task<IActionResult> Decline(int groupId, int id)
        {
            return await RespondAsync(groupId, id, "Declined");
        }

        private async Task<IActionResult> RespondAsync(int groupId, int id, string status)
        {
            var userId = CurrentUserId;

            var duel = await _context.Duels.FirstOrDefaultAsync(d => d.Id == id && d.GroupId == groupId);
            if (duel == null)
            {
                return NotFound();
            }

            if (duel.OpponentUserId != userId)
            {
                return StatusCode(403, new { message = "Solo la persona retada puede responder el duelo." });
            }

            if (duel.Status != "Pending")
            {
                return Conflict(new { message = "Este duelo ya fue respondido." });
            }

            duel.Status = status;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpGet("duels/mine")]
        public async Task<ActionResult<IEnumerable<DuelDto>>> Mine(int groupId)
        {
            var userId = CurrentUserId;

            var ids = await _context.Duels.AsNoTracking()
                .Where(d => d.GroupId == groupId && (d.ChallengerUserId == userId || d.OpponentUserId == userId))
                .OrderByDescending(d => d.Id)
                .Select(d => d.Id)
                .ToListAsync();

            return Ok(await BuildDuelsAsync(groupId, userId, ids));
        }

        private async Task<List<DuelDto>> BuildDuelsAsync(int groupId, string userId, List<int> duelIds)
        {
            if (duelIds.Count == 0)
            {
                return new List<DuelDto>();
            }

            var duels = await _context.Duels.AsNoTracking()
                .Where(d => duelIds.Contains(d.Id))
                .Include(d => d.Match)
                .Include(d => d.Challenger)
                .Include(d => d.Opponent)
                .ToListAsync();

            var teams = await _context.Teams.AsNoTracking()
                .Where(t => t.League.GroupId == groupId)
                .ToDictionaryAsync(t => t.Id, t => t.Initials);

            var matchIds = duels.Select(d => d.MatchId).Distinct().ToList();
            var userIds = duels.SelectMany(d => new[] { d.ChallengerUserId, d.OpponentUserId }).Distinct().ToList();
            var predictions = await _context.Predictions.AsNoTracking()
                .Where(p => matchIds.Contains(p.MatchId) && userIds.Contains(p.CreatedBy))
                .Select(p => new { p.MatchId, p.CreatedBy, p.AdquiredPoints })
                .ToListAsync();

            var result = new List<DuelDto>();
            foreach (var duel in duels.OrderByDescending(d => d.Id))
            {
                var closed = duel.Match != null && duel.Match.StatusId == StatusTerminado;
                int? challengerPoints = null;
                int? opponentPoints = null;
                string winnerName = null;

                if (closed && duel.Status == "Accepted")
                {
                    challengerPoints = predictions.FirstOrDefault(p => p.MatchId == duel.MatchId && p.CreatedBy == duel.ChallengerUserId)?.AdquiredPoints ?? 0;
                    opponentPoints = predictions.FirstOrDefault(p => p.MatchId == duel.MatchId && p.CreatedBy == duel.OpponentUserId)?.AdquiredPoints ?? 0;

                    if (challengerPoints > opponentPoints)
                    {
                        winnerName = duel.Challenger?.FullName;
                    }
                    else if (opponentPoints > challengerPoints)
                    {
                        winnerName = duel.Opponent?.FullName;
                    }
                }

                var local = duel.Match != null && teams.TryGetValue(duel.Match.LocalId, out var l) ? l : "?";
                var visitor = duel.Match != null && teams.TryGetValue(duel.Match.VisitorId, out var v) ? v : "?";

                string statusLabel;
                if (duel.Status == "Declined")
                {
                    statusLabel = "Rechazado";
                }
                else if (duel.Status == "Pending")
                {
                    statusLabel = "Pendiente de respuesta";
                }
                else if (closed)
                {
                    var outcome = winnerName != null ? $"Ganó {winnerName}" : "Empate";
                    statusLabel = $"{challengerPoints} - {opponentPoints} · {outcome}";
                }
                else
                {
                    statusLabel = "Aceptado — esperando el partido";
                }

                result.Add(new DuelDto
                {
                    Id = duel.Id,
                    MatchId = duel.MatchId,
                    MatchLabel = $"{local} vs {visitor}",
                    ChallengerUserId = duel.ChallengerUserId,
                    ChallengerName = duel.Challenger?.FullName,
                    OpponentUserId = duel.OpponentUserId,
                    OpponentName = duel.Opponent?.FullName,
                    Status = duel.Status,
                    MatchClosed = closed,
                    ChallengerPoints = challengerPoints,
                    OpponentPoints = opponentPoints,
                    WinnerName = winnerName,
                    StatusLabel = statusLabel,
                    CanRespond = duel.OpponentUserId == userId && duel.Status == "Pending"
                });
            }

            return result;
        }
    }
}
