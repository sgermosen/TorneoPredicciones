namespace CompeTournament.Backend.Controllers.Api
{
    using CompeTournament.Backend.Data;
    using CompeTournament.Backend.Data.Entities;
    using CompeTournament.Backend.Helpers;
    using CompeTournament.Shared.Tournaments;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    [Route("api/groups")]
    public class GroupsApiController : ApiControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IJornadaRecapGenerator _recapGenerator;
        private readonly IUserHelper _userHelper;

        public GroupsApiController(ApplicationDbContext context, IJornadaRecapGenerator recapGenerator, IUserHelper userHelper)
        {
            _context = context;
            _recapGenerator = recapGenerator;
            _userHelper = userHelper;
        }

        [HttpGet]
        public async Task<ActionResult<List<GroupDto>>> GetAll()
        {
            var userId = CurrentUserId;
            var groups = await _context.Groups
                .Include(g => g.TournamentType)
                .Include(g => g.GroupUsers)
                .Where(g => g.IsPublic || g.GroupUsers.Any(gu => gu.ApplicationUserId == userId && !gu.IsBlocked))
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

        [HttpPost]
        public async Task<ActionResult<GroupDto>> Create([FromBody] CreateGroupRequest request)
        {
            var userId = CurrentUserId;

            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return BadRequest(new { message = "Escribe el nombre del torneo." });
            }

            var typeExists = await _context.TournamentTypes.AnyAsync(t => t.Id == request.TournamentTypeId);
            if (!typeExists)
            {
                return BadRequest(new { message = "Elige un tipo de torneo válido." });
            }

            var group = new Group
            {
                Name = request.Name.Trim(),
                Requirements = request.Requirements,
                TournamentTypeId = request.TournamentTypeId,
                InviteCode = InviteCodeGenerator.Generate(),
                Active = true,
                IsPublic = request.IsPublic ?? true,
                CreditsEnabled = request.CreditsEnabled ?? false,
                InitialCredits = Math.Max(0, request.InitialCredits ?? 0)
            };
            _context.Groups.Add(group);
            await _context.SaveChangesAsync();

            _context.GroupUsers.Add(new GroupUser
            {
                GroupId = group.Id,
                ApplicationUserId = userId,
                IsAccepted = true,
                IsBlocked = false,
                Points = 0
            });
            await _context.SaveChangesAsync();

            await GrantInitialCreditsIfNeededAsync(group.Id, userId);

            var created = await _context.Groups
                .Include(g => g.TournamentType)
                .Include(g => g.GroupUsers)
                .AsNoTracking()
                .FirstAsync(g => g.Id == group.Id);

            return Ok(MapGroup(created, userId));
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

            var tournaments = await _context.Tournaments
                .Where(t => t.GroupId == id)
                .OrderBy(t => t.Name)
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
                IsMember = membership != null && !membership.IsBlocked,
                IsAccepted = membership != null && membership.IsAccepted && !membership.IsBlocked,
                CanManage = CanManageOwnedBy(group.CreatedBy),
                IsPublic = group.IsPublic,
                CreditsEnabled = group.CreditsEnabled,
                MyCredits = membership?.Credits ?? 0,
                Matches = matches.Select(m => MapMatch(m, predictions.FirstOrDefault(p => p.MatchId == m.Id))).ToList(),
                Standings = standings.Select(MapStanding).ToList(),
                Tournaments = tournaments.Select(t => new TournamentDto { Id = t.Id, Name = t.Name }).ToList()
            };

            return Ok(detail);
        }

        [HttpPost("{id:int}/join")]
        public async Task<IActionResult> Join(int id)
        {
            var userId = CurrentUserId;

            var group = await _context.Groups.AsNoTracking().FirstOrDefaultAsync(g => g.Id == id);
            if (group == null)
            {
                return NotFound();
            }

            if (!group.IsPublic)
            {
                return StatusCode(403, new { message = "Este torneo es privado; necesitas una invitación." });
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
                .AnyAsync(gu => gu.GroupId == id && gu.ApplicationUserId == userId && gu.IsAccepted && !gu.IsBlocked);
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
                return NotFound(new { message = "Código de invitación inválido." });
            }

            var existing = group.GroupUsers.FirstOrDefault(gu => gu.ApplicationUserId == userId);
            if (existing != null && existing.IsBlocked)
            {
                return Forbid();
            }

            if (existing != null && !existing.IsAccepted)
            {
                existing.IsAccepted = true;
                await _context.SaveChangesAsync();
            }

            if (existing == null)
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

            await GrantInitialCreditsIfNeededAsync(group.Id, userId);

            return Ok(MapGroup(group, userId));
        }

        [HttpGet("{id:int}/recap")]
        public async Task<ActionResult<RecapDto>> Recap(int id)
        {
            var group = await _context.Groups.FirstOrDefaultAsync(g => g.Id == id);
            if (group == null)
            {
                return NotFound();
            }

            var leaderboard = await _context.GroupUsers
                .Where(gu => gu.GroupId == id && gu.IsAccepted && !gu.IsBlocked)
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
                .AsNoTracking()
                .ToListAsync();

            var lastMatch = await _context.Matches
                .Where(m => m.GroupId == id && m.StatusId == 3)
                .Include(m => m.Local)
                .Include(m => m.Visitor)
                .OrderByDescending(m => m.DateTime)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            string lastMatchSummary = null;
            if (lastMatch != null)
            {
                lastMatchSummary = $"{lastMatch.Local?.Initials} {lastMatch.LocalPoints}-{lastMatch.VisitorPoints} {lastMatch.Visitor?.Initials}";
            }

            var text = await _recapGenerator.GenerateAsync(new RecapContext
            {
                GroupName = group.Name,
                Leaderboard = leaderboard,
                LastMatchSummary = lastMatchSummary
            });

            return Ok(new RecapDto { GroupId = id, Text = text });
        }

        [HttpGet("{id:int}/leaderboard")]
        public async Task<ActionResult<List<LeaderboardEntryDto>>> Leaderboard(int id)
        {
            if (!await _context.Groups.AnyAsync(g => g.Id == id))
            {
                return NotFound();
            }

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

        [HttpGet("{id:int}/members")]
        public async Task<ActionResult<List<LeaderboardEntryDto>>> Members(int id)
        {
            var group = await _context.Groups
                .Include(g => g.GroupUsers)
                    .ThenInclude(gu => gu.ApplicationUser)
                .AsNoTracking()
                .FirstOrDefaultAsync(g => g.Id == id);

            if (group == null)
            {
                return NotFound();
            }

            if (!CanManageOwnedBy(group.CreatedBy))
            {
                return StatusCode(403, new { message = "No gestionas este torneo." });
            }

            return Ok(group.GroupUsers
                .OrderByDescending(gu => gu.IsAccepted && !gu.IsBlocked)
                .ThenByDescending(gu => gu.Points)
                .Select(gu => new LeaderboardEntryDto
                {
                    UserId = gu.ApplicationUserId,
                    FullName = gu.ApplicationUser != null ? gu.ApplicationUser.FullName : null,
                    Points = gu.Points,
                    Credits = gu.Credits,
                    IsAccepted = gu.IsAccepted,
                    IsBlocked = gu.IsBlocked
                })
                .ToList());
        }

        [HttpPost("{id:int}/members/{userId}/accept")]
        public async Task<IActionResult> AcceptMember(int id, string userId)
        {
            var group = await _context.Groups.AsNoTracking().FirstOrDefaultAsync(g => g.Id == id);
            if (group == null)
            {
                return NotFound();
            }

            if (!CanManageOwnedBy(group.CreatedBy))
            {
                return StatusCode(403, new { message = "No gestionas este torneo." });
            }

            var member = await _context.GroupUsers
                .FirstOrDefaultAsync(gu => gu.GroupId == id && gu.ApplicationUserId == userId);
            if (member == null)
            {
                return NotFound(new { message = "La persona no tiene solicitud en este torneo." });
            }

            member.IsAccepted = true;
            await _context.SaveChangesAsync();

            await GrantInitialCreditsIfNeededAsync(id, userId);

            return NoContent();
        }

        [HttpPost("{id:int}/members/{userId}/block")]
        public async Task<IActionResult> BlockMember(int id, string userId)
        {
            var group = await _context.Groups.AsNoTracking().FirstOrDefaultAsync(g => g.Id == id);
            if (group == null)
            {
                return NotFound();
            }

            if (!CanManageOwnedBy(group.CreatedBy))
            {
                return StatusCode(403, new { message = "No gestionas este torneo." });
            }

            var member = await _context.GroupUsers
                .FirstOrDefaultAsync(gu => gu.GroupId == id && gu.ApplicationUserId == userId);
            if (member == null)
            {
                return NotFound(new { message = "La persona no es miembro de este torneo." });
            }

            member.IsBlocked = true;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPost("{id:int}/members/invite")]
        public async Task<ActionResult<LeaderboardEntryDto>> InviteMember(int id, [FromBody] InviteMemberRequest request)
        {
            var group = await _context.Groups.AsNoTracking().FirstOrDefaultAsync(g => g.Id == id);
            if (group == null)
            {
                return NotFound();
            }

            if (!CanManageOwnedBy(group.CreatedBy))
            {
                return StatusCode(403, new { message = "No gestionas este torneo." });
            }

            var email = request.Email?.Trim();
            if (string.IsNullOrWhiteSpace(email))
            {
                return BadRequest(new { message = "Escribe el correo de la persona." });
            }

            var user = await _userHelper.GetUserByEmailAsync(email);
            if (user == null)
            {
                return NotFound(new { message = "No hay un usuario con ese correo." });
            }

            var membership = await _context.GroupUsers
                .FirstOrDefaultAsync(gu => gu.GroupId == id && gu.ApplicationUserId == user.Id);

            if (membership == null)
            {
                membership = new GroupUser
                {
                    GroupId = id,
                    ApplicationUserId = user.Id,
                    IsAccepted = true,
                    IsBlocked = false,
                    Points = 0
                };
                _context.GroupUsers.Add(membership);
                await _context.SaveChangesAsync();
            }
            else if (membership.IsBlocked)
            {
                return Conflict(new { message = "Esa persona está bloqueada en este torneo." });
            }
            else if (membership.IsAccepted)
            {
                return Conflict(new { message = "Esa persona ya es miembro de este torneo." });
            }
            else
            {
                membership.IsAccepted = true;
                await _context.SaveChangesAsync();
            }

            await GrantInitialCreditsIfNeededAsync(id, user.Id);

            return Ok(new LeaderboardEntryDto
            {
                UserId = user.Id,
                FullName = user.FullName,
                Points = membership.Points,
                Credits = membership.Credits,
                IsAccepted = membership.IsAccepted,
                IsBlocked = membership.IsBlocked
            });
        }

        [HttpPost("{id:int}/visibility")]
        public async Task<IActionResult> SetVisibility(int id, [FromBody] SetVisibilityRequest request)
        {
            var group = await _context.Groups.FirstOrDefaultAsync(g => g.Id == id);
            if (group == null)
            {
                return NotFound();
            }

            if (!CanManageOwnedBy(group.CreatedBy))
            {
                return StatusCode(403, new { message = "No gestionas este torneo." });
            }

            group.IsPublic = request.IsPublic;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("{id:int}/tournaments")]
        public async Task<ActionResult<List<TournamentDto>>> Tournaments(int id)
        {
            var group = await _context.Groups.AsNoTracking().FirstOrDefaultAsync(g => g.Id == id);
            if (group == null)
            {
                return NotFound();
            }

            if (!CanManageOwnedBy(group.CreatedBy))
            {
                return StatusCode(403, new { message = "No gestionas este torneo." });
            }

            var tournaments = await _context.Tournaments
                .Where(t => t.GroupId == id)
                .OrderBy(t => t.Name)
                .Select(t => new TournamentDto { Id = t.Id, Name = t.Name })
                .AsNoTracking()
                .ToListAsync();

            return Ok(tournaments);
        }

        [HttpPost("{id:int}/tournaments")]
        public async Task<ActionResult<TournamentDto>> CreateTournament(int id, [FromBody] CreateTournamentRequest request)
        {
            var group = await _context.Groups.AsNoTracking().FirstOrDefaultAsync(g => g.Id == id);
            if (group == null)
            {
                return NotFound();
            }

            if (!CanManageOwnedBy(group.CreatedBy))
            {
                return StatusCode(403, new { message = "No gestionas este torneo." });
            }

            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return BadRequest(new { message = "Escribe el nombre del torneo interno." });
            }

            var tournament = new Tournament
            {
                GroupId = id,
                Name = request.Name.Trim(),
                Active = true
            };
            _context.Tournaments.Add(tournament);
            await _context.SaveChangesAsync();

            return Ok(new TournamentDto { Id = tournament.Id, Name = tournament.Name });
        }

        [HttpGet("{id:int}/leagues")]
        public async Task<ActionResult<List<LeagueDto>>> Leagues(int id)
        {
            var group = await _context.Groups.AsNoTracking().FirstOrDefaultAsync(g => g.Id == id);
            if (group == null)
            {
                return NotFound();
            }

            if (!CanManageOwnedBy(group.CreatedBy))
            {
                return StatusCode(403, new { message = "No gestionas este torneo." });
            }

            var leagues = await _context.Leagues
                .Where(l => l.GroupId == id)
                .OrderBy(l => l.Name)
                .Select(l => new LeagueDto { Id = l.Id, Name = l.Name })
                .AsNoTracking()
                .ToListAsync();

            return Ok(leagues);
        }

        [HttpPost("{id:int}/leagues")]
        public async Task<ActionResult<LeagueDto>> CreateLeague(int id, [FromBody] CreateLeagueRequest request)
        {
            var group = await _context.Groups.AsNoTracking().FirstOrDefaultAsync(g => g.Id == id);
            if (group == null)
            {
                return NotFound();
            }

            if (!CanManageOwnedBy(group.CreatedBy))
            {
                return StatusCode(403, new { message = "No gestionas este torneo." });
            }

            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return BadRequest(new { message = "Escribe el nombre de la liga." });
            }

            var league = new League
            {
                GroupId = id,
                Name = request.Name.Trim(),
                Active = true
            };
            _context.Leagues.Add(league);
            await _context.SaveChangesAsync();

            return Ok(new LeagueDto { Id = league.Id, Name = league.Name });
        }

        [HttpPost("{id:int}/teams")]
        public async Task<ActionResult<StandingDto>> CreateTeam(int id, [FromBody] CreateTeamRequest request)
        {
            var group = await _context.Groups.AsNoTracking().FirstOrDefaultAsync(g => g.Id == id);
            if (group == null)
            {
                return NotFound();
            }

            if (!CanManageOwnedBy(group.CreatedBy))
            {
                return StatusCode(403, new { message = "No gestionas este torneo." });
            }

            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return BadRequest(new { message = "Escribe el nombre del equipo." });
            }

            var leagueInGroup = await _context.Leagues.AnyAsync(l => l.Id == request.LeagueId && l.GroupId == id);
            if (!leagueInGroup)
            {
                return BadRequest(new { message = "Elige una liga de este torneo." });
            }

            var team = new Team
            {
                LeagueId = request.LeagueId,
                Name = request.Name.Trim(),
                Initials = string.IsNullOrWhiteSpace(request.Initials)
                    ? new string(request.Name.Trim().Replace(" ", string.Empty).Take(3).ToArray()).ToUpperInvariant()
                    : request.Initials.Trim().ToUpperInvariant(),
                Active = true,
                MatchesPlayed = 0,
                MatchesWon = 0,
                MatchesTied = 0,
                MatchesLost = 0,
                FavorPoints = 0,
                AgainstPoints = 0,
                CumulativePoints = 0,
                Position = 0
            };
            _context.Teams.Add(team);
            await _context.SaveChangesAsync();

            return Ok(MapStanding(team));
        }

        [HttpPost("{id:int}/matches")]
        public async Task<ActionResult<MatchDto>> CreateMatch(int id, [FromBody] CreateMatchRequest request)
        {
            var group = await _context.Groups.AsNoTracking().FirstOrDefaultAsync(g => g.Id == id);
            if (group == null)
            {
                return NotFound();
            }

            if (!CanManageOwnedBy(group.CreatedBy))
            {
                return StatusCode(403, new { message = "No gestionas este torneo." });
            }

            if (request.LocalId == request.VisitorId)
            {
                return BadRequest(new { message = "El local y el visitante deben ser equipos distintos." });
            }

            var teamsInGroup = await _context.Teams
                .CountAsync(t => (t.Id == request.LocalId || t.Id == request.VisitorId) && t.League.GroupId == id);
            if (teamsInGroup < 2)
            {
                return BadRequest(new { message = "Elige equipos de las ligas de este torneo." });
            }

            var kickoff = DateTime.SpecifyKind(request.DateTime, DateTimeKind.Utc);
            if (kickoff <= DateTime.UtcNow)
            {
                return BadRequest(new { message = "La fecha y hora del partido deben ser futuras." });
            }

            if (request.TournamentId.HasValue)
            {
                var tournamentInGroup = await _context.Tournaments.AnyAsync(t => t.Id == request.TournamentId.Value && t.GroupId == id);
                if (!tournamentInGroup)
                {
                    return BadRequest(new { message = "Elige un torneo interno de este grupo." });
                }
            }

            var match = new Match
            {
                GroupId = id,
                TournamentId = request.TournamentId,
                Name = string.IsNullOrWhiteSpace(request.Name) ? "Partido" : request.Name.Trim(),
                LocalId = request.LocalId,
                VisitorId = request.VisitorId,
                DateTime = kickoff,
                StatusId = 1,
                Active = true
            };
            _context.Matches.Add(match);
            await _context.SaveChangesAsync();

            var created = await _context.Matches
                .Include(m => m.Local)
                .Include(m => m.Visitor)
                .Include(m => m.Status)
                .AsNoTracking()
                .FirstAsync(m => m.Id == match.Id);

            return Ok(MapMatch(created, null));
        }

        [HttpPost("{id:int}/members/{userId}/credits")]
        public async Task<ActionResult<LeaderboardEntryDto>> AdjustCredits(int id, string userId, [FromBody] AdjustCreditsRequest request)
        {
            var group = await _context.Groups.AsNoTracking().FirstOrDefaultAsync(g => g.Id == id);
            if (group == null)
            {
                return NotFound();
            }

            if (!CanManageOwnedBy(group.CreatedBy))
            {
                return StatusCode(403, new { message = "No gestionas este torneo." });
            }

            if (!group.CreditsEnabled)
            {
                return BadRequest(new { message = "Este torneo no tiene fichas activadas." });
            }

            var member = await _context.GroupUsers
                .Include(gu => gu.ApplicationUser)
                .FirstOrDefaultAsync(gu => gu.GroupId == id && gu.ApplicationUserId == userId);
            if (member == null)
            {
                return NotFound(new { message = "La persona no es miembro de este torneo." });
            }

            if (member.Credits + request.Amount < 0)
            {
                return BadRequest(new { message = "La persona no tiene suficientes fichas para ese ajuste." });
            }

            member.Credits += request.Amount;
            _context.CreditTransactions.Add(new CreditTransaction
            {
                GroupUserId = member.Id,
                Amount = request.Amount,
                BalanceAfter = member.Credits,
                Reason = "AdminAdjustment"
            });
            await _context.SaveChangesAsync();

            return Ok(new LeaderboardEntryDto
            {
                UserId = member.ApplicationUserId,
                FullName = member.ApplicationUser != null ? member.ApplicationUser.FullName : null,
                Points = member.Points,
                Credits = member.Credits,
                IsAccepted = member.IsAccepted,
                IsBlocked = member.IsBlocked
            });
        }

        private async Task GrantInitialCreditsIfNeededAsync(int groupId, string userId)
        {
            var group = await _context.Groups.AsNoTracking().FirstOrDefaultAsync(g => g.Id == groupId);
            if (group == null || !group.CreditsEnabled || group.InitialCredits <= 0)
            {
                return;
            }

            var member = await _context.GroupUsers
                .FirstOrDefaultAsync(gu => gu.GroupId == groupId && gu.ApplicationUserId == userId);
            if (member == null || !member.IsAccepted || member.IsBlocked)
            {
                return;
            }

            var alreadyGranted = await _context.CreditTransactions
                .AnyAsync(ct => ct.GroupUserId == member.Id && ct.Reason == "InitialGrant");
            if (alreadyGranted)
            {
                return;
            }

            member.Credits += group.InitialCredits;
            _context.CreditTransactions.Add(new CreditTransaction
            {
                GroupUserId = member.Id,
                Amount = group.InitialCredits,
                BalanceAfter = member.Credits,
                Reason = "InitialGrant"
            });
            await _context.SaveChangesAsync();
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
                IsMember = membership != null && !membership.IsBlocked,
                IsAccepted = membership != null && membership.IsAccepted && !membership.IsBlocked,
                IsPublic = group.IsPublic,
                MyPoints = membership?.Points ?? 0
            };
        }

        private static MatchDto MapMatch(Match match, Prediction prediction)
        {
            return new MatchDto
            {
                Id = match.Id,
                DateTime = DateTime.SpecifyKind(match.DateTime, DateTimeKind.Utc),
                GroupId = match.GroupId,
                TournamentId = match.TournamentId,
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
