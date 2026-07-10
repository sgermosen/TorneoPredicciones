namespace CompeTournament.Backend.Controllers.Api
{
    using CompeTournament.Backend.Data;
    using CompeTournament.Backend.Data.Entities;
    using CompeTournament.Backend.Realtime;
    using CompeTournament.Shared.Tournaments;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.SignalR;
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    [Route("api/matches/{matchId:int}/comments")]
    public class CommentsApiController : ApiControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<TournamentHub> _hub;

        public CommentsApiController(ApplicationDbContext context, IHubContext<TournamentHub> hub)
        {
            _context = context;
            _hub = hub;
        }

        [HttpGet]
        public async Task<ActionResult<List<CommentDto>>> Get(int matchId)
        {
            var comments = await _context.MatchComments
                .Where(c => c.MatchId == matchId)
                .Include(c => c.ApplicationUser)
                .OrderBy(c => c.CreatedAt)
                .Select(c => new CommentDto
                {
                    Id = c.Id,
                    MatchId = c.MatchId,
                    AuthorName = c.ApplicationUser != null ? c.ApplicationUser.FullName : null,
                    Body = c.Body,
                    CreatedAt = c.CreatedAt
                })
                .AsNoTracking()
                .ToListAsync();

            return Ok(comments);
        }

        [HttpPost]
        public async Task<ActionResult<CommentDto>> Post(int matchId, [FromBody] CommentRequest request)
        {
            var userId = CurrentUserId;

            var match = await _context.Matches.FirstOrDefaultAsync(m => m.Id == matchId);
            if (match == null)
            {
                return NotFound();
            }

            var isMember = await _context.GroupUsers
                .AnyAsync(gu => gu.GroupId == match.GroupId && gu.ApplicationUserId == userId && gu.IsAccepted && !gu.IsBlocked);
            if (!isMember)
            {
                return Forbid();
            }

            var comment = new MatchComment
            {
                MatchId = matchId,
                ApplicationUserId = userId,
                Body = request.Body,
                CreatedAt = DateTime.UtcNow
            };
            _context.MatchComments.Add(comment);
            await _context.SaveChangesAsync();

            var author = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            var dto = new CommentDto
            {
                Id = comment.Id,
                MatchId = matchId,
                AuthorName = author?.FullName,
                Body = comment.Body,
                CreatedAt = comment.CreatedAt
            };

            await _hub.Clients.Group(TournamentHub.GroupName(match.GroupId)).SendAsync("CommentPosted", dto);

            return Ok(dto);
        }
    }
}
