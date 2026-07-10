namespace CompeTournament.Backend.Controllers.Api
{
    using CompeTournament.Backend.Data;
    using CompeTournament.Backend.Data.Entities;
    using CompeTournament.Backend.Helpers;
    using CompeTournament.Shared.Tournaments;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    [Route("api/groups")]
    public class GroupsApiController : ApiControllerBase
    {
        private readonly ApplicationDbContext _context;

        public GroupsApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<List<GroupDto>>> GetAll()
        {
            var userId = CurrentUserId;
            var groups = await _context.Groups
                .Include(g => g.TournamentType)
                .Include(g => g.GroupUsers)
                .AsNoTracking()
                .ToListAsync();

            return Ok(groups.Select(g => MapGroup(g, userId)).ToList());
        }

        [HttpGet("mine")]
        public async Task<ActionResult<List<GroupDto>>> GetMine()
        {
            var userId = CurrentUserId;
            var groups = await _context.Groups
                .Include(g => g.TournamentType)
                .Include(g => g.GroupUsers)
                .Where(g => g.GroupUsers.Any(gu => gu.ApplicationUserId == userId && !gu.IsBlocked))
                .AsNoTracking()
                .ToListAsync();

            return Ok(groups.Select(g => MapGroup(g, userId)).ToList());
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<GroupDetailDto>> GetById(int id)
        {
            var userId = CurrentUserId;

            var group = await _context.Groups
                .Include(g => g.TournamentType)
                .Include(g => g.GroupUsers)
                .Where(g => g.Id == id)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (group == null)
            {
                return NotFound();
            }

            var matches = await _context.Matches
                .Where(m => m.GroupId == id)
                .Include(m => m.Local)
                .Include(m => m.Visitor)
                .Include(m => m.Status)
                .OrderBy(m => m.DateTime)
                .AsNoTracking()
                .ToListAsync();

            var predictions = await _context.Predictions
                .Where(p => p.CreatedBy == userId && p.Match.GroupId == id)
                .AsNoTracking()
                .ToListAsync();

            var standings = await _context.Teams
                .Where(t => t.League.GroupId == id)
                .OrderBy(t => t.Position)
                .AsNoTracking()
                .ToListAsync();

            var membership = group.GroupUsers.FirstOrDefault(gu => gu.ApplicationUserId == userId);

            var detail = new GroupDetailDto
            {
                Id = group.Id,
                Name = group.Name,
                Requirements = group.Requirements,
                Logo = group.Logo,
                TournamentTypeName = group.TournamentType?.Name,
                IsMember = membership != null,
                IsAccepted = membership != null && membership.IsAccepted,
                Matches = matches.Select(m => MapMatch(m, predictions.FirstOrDefault(p => p.MatchId == m.Id))).ToList(),
                Standings = standings.Select(MapStanding).ToList()
            };

            return Ok(detail);
        }

        [HttpPost("{id:int}/join")]
        public async Task<IActionResult> Join(int id)
        {
            var userId = CurrentUserId;

            var groupExists = await _context.Groups.AnyAsync(g => g.Id == id);
            if (!groupExists)
            {
                return NotFound();
            }

            var alreadyRequested = await _context.GroupUsers
                .AnyAsync(gu => gu.GroupId == id && gu.ApplicationUserId == userId);
            if (alreadyRequested)
            {
                return Conflict(new { message = "Ya existe una solicitud para este grupo." });
            }

            _context.GroupUsers.Add(new GroupUser
            {
                GroupId = id,
                ApplicationUserId = userId,
                IsAccepted = false,
                IsBlocked = false,
                Points = 0
            });
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("{id:int}/invite")]
        public async Task<ActionResult<GroupInviteDto>> Invite(int id)
        {
            var userId = CurrentUserId;

            var group = await _context.Groups.FirstOrDefaultAsync(g => g.Id == id);
            if (group == null)
            {
                return NotFound();
            }

            var isMember = await _context.GroupUsers
                .AnyAsync(gu => gu.GroupId == id && gu.ApplicationUserId == userId && !gu.IsBlocked);
            if (!isMember)
            {
                return Forbid();
            }

            if (string.IsNullOrEmpty(group.InviteCode))
            {
                group.InviteCode = InviteCodeGenerator.Generate();
                await _context.SaveChangesAsync();
            }

            return Ok(new GroupInviteDto
            {
                GroupId = group.Id,
                GroupName = group.Name,
                Code = group.InviteCode
            });
        }

        [HttpPost("join-by-code")]
        public async Task<ActionResult<GroupDto>> JoinByCode([FromBody] JoinByCodeRequest request)
        {
            var userId = CurrentUserId;
            var code = request.Code.Trim().ToUpperInvariant();

            var group = await _context.Groups
                .Include(g => g.TournamentType)
                .Include(g => g.GroupUsers)
                .FirstOrDefaultAsync(g => g.InviteCode == code);

            if (group == null)
            {
                return NotFound(new { message = "Codigo de invitacion invalido." });
            }

            if (group.GroupUsers.All(gu => gu.ApplicationUserId != userId))
            {
                var membership = new GroupUser
                {
                    GroupId = group.Id,
                    ApplicationUserId = userId,
                    IsAccepted = true,
                    IsBlocked = false,
                    Points = 0
                };
                _context.GroupUsers.Add(membership);
                await _context.SaveChangesAsync();
                group.GroupUsers.Add(membership);
            }

            return Ok(MapGroup(group, userId));
        }

        [HttpGet("{id:int}/leaderboard")]
        public async Task<ActionResult<List<LeaderboardEntryDto>>> Leaderboard(int id)
        {
            var members = await _context.GroupUsers
                .Where(gu => gu.GroupId == id && gu.IsAccepted && !gu.IsBlocked)
                .Include(gu => gu.ApplicationUser)
                .OrderByDescending(gu => gu.Points)
                .AsNoTracking()
                .ToListAsync();

            return Ok(members.Select(gu => new LeaderboardEntryDto
            {
                UserId = gu.ApplicationUserId,
                FullName = gu.ApplicationUser != null ? gu.ApplicationUser.FullName : null,
                Points = gu.Points,
                IsAccepted = gu.IsAccepted,
                IsBlocked = gu.IsBlocked
            }).ToList());
        }

        private static GroupDto MapGroup(Group group, string userId)
        {
            var membership = group.GroupUsers?.FirstOrDefault(gu => gu.ApplicationUserId == userId);
            return new GroupDto
            {
                Id = group.Id,
                Name = group.Name,
                Requirements = group.Requirements,
                Logo = group.Logo,
                TournamentTypeName = group.TournamentType?.Name,
                MembersCount = group.GroupUsers?.Count(gu => gu.IsAccepted && !gu.IsBlocked) ?? 0,
                IsMember = membership != null,
                IsAccepted = membership != null && membership.IsAccepted
            };
        }

        private static MatchDto MapMatch(Match match, Prediction prediction)
        {
            return new MatchDto
            {
                Id = match.Id,
                DateTime = match.DateTime,
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
                    AdquiredPoints = prediction.AdquiredPoints
                }
            };
        }

        private static StandingDto MapStanding(Team team)
        {
            return new StandingDto
            {
                TeamId = team.Id,
                Position = team.Position,
                Name = team.Name,
                Initials = team.Initials,
                PictureUrl = team.PictureUrl,
                MatchesPlayed = team.MatchesPlayed,
                MatchesWon = team.MatchesWon,
                MatchesTied = team.MatchesTied,
                MatchesLost = team.MatchesLost,
                FavorPoints = team.FavorPoints,
                AgainstPoints = team.AgainstPoints,
                CumulativePoints = team.CumulativePoints
            };
        }
    }
}
