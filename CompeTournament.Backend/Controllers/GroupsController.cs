using CompeTournament.Backend.Data;
using CompeTournament.Backend.Data.Entities;
using CompeTournament.Backend.Helpers;
using CompeTournament.Backend.Persistence.Contracts;
using CompeTournament.Shared.Scoring;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CompeTournament.Backend.Controllers
{
    [Authorize]
    [AutoValidateAntiforgeryToken]
    public class GroupsController : Controller
    {
        private readonly IMatchClosingService _matchClosingService;

        private readonly IGroupRepository _groupRepo;
        private readonly ITournamentTypeRepository _typeRepository;
        private readonly ILeagueRepository _leagueRepo;
        private readonly ITeamRepository _teamRepository;
        private readonly IMatchRepository _matchRepository;
        private readonly ApplicationDbContext _context;
        private readonly TimeZoneInfo _timeZone;

        public GroupsController(IGroupRepository groupRepo, ITournamentTypeRepository typeRepository
            , ILeagueRepository leagueRepo, ITeamRepository teamRepository, IMatchRepository matchRepository,
            IMatchClosingService matchClosingService, ApplicationDbContext context, TimeZoneInfo timeZone)
        {
            _groupRepo = groupRepo;
            _typeRepository = typeRepository;
            _leagueRepo = leagueRepo;
            _teamRepository = teamRepository;
            _matchRepository = matchRepository;
            _matchClosingService = matchClosingService;
            _context = context;
            _timeZone = timeZone;
        }

        #region Prediction
        public async Task<IActionResult> MakePrediction(int id)
        {
            var match = await _matchRepository.GetByIdWithChildrens(id);
            if (match == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isMember = await _context.GroupUsers
                .AnyAsync(gu => gu.GroupId == match.GroupId && gu.ApplicationUserId == userId && gu.IsAccepted && !gu.IsBlocked);
            if (!isMember)
            {
                return Forbid();
            }

            if (match.StatusId != 1)
            {
                TempData["Message"] = "Este partido ya no admite predicciones.";
                return RedirectToAction(nameof(Details), new { id = match.GroupId });
            }

            var prediction = await _context.Predictions
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.MatchId == id && p.CreatedBy == userId)
                ?? new Prediction { MatchId = id };

            ViewData["Match"] = match;
            return View(prediction);
        }

        [HttpPost]
        public async Task<IActionResult> MakePrediction(Prediction model)
        {
            var match = await _matchRepository.GetByIdWithChildrens(model.MatchId);
            if (match == null)
            {
                return NotFound();
            }

            ViewData["Match"] = match;
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isMember = await _context.GroupUsers
                .AnyAsync(gu => gu.GroupId == match.GroupId && gu.ApplicationUserId == userId && gu.IsAccepted && !gu.IsBlocked);
            if (!isMember)
            {
                ModelState.AddModelError(string.Empty, "Solo los miembros aprobados del torneo pueden predecir.");
            }

            if (match.StatusId != 1)
            {
                ModelState.AddModelError(string.Empty, "Este partido ya no admite predicciones.");
            }

            if (model.LocalPoints == null || model.VisitorPoints == null || model.LocalPoints < 0 || model.VisitorPoints < 0)
            {
                ModelState.AddModelError(string.Empty, "Escribe el marcador de los dos equipos.");
            }

            if (!this.ModelState.IsValid)
            {
                return this.View(model);
            }

            var existing = await _context.Predictions
                .FirstOrDefaultAsync(p => p.MatchId == model.MatchId && p.CreatedBy == userId);
            if (existing == null)
            {
                model.AdquiredPoints = 0;
                model.Id = 0;
                await _matchRepository.AddPrediction(model);
            }
            else
            {
                existing.LocalPoints = model.LocalPoints;
                existing.VisitorPoints = model.VisitorPoints;
                existing.IsBanker = model.IsBanker;
                await _context.SaveChangesAsync();
            }

            if (model.IsBanker)
            {
                var others = await _context.Predictions
                    .Where(p => p.CreatedBy == userId && p.MatchId != model.MatchId && p.Match.GroupId == match.GroupId && p.Match.StatusId == 1 && p.IsBanker)
                    .ToListAsync();
                foreach (var other in others)
                {
                    other.IsBanker = false;
                }
                await _context.SaveChangesAsync();
            }

            TempData["Message"] = "Tu predicción quedó guardada.";
            return this.RedirectToAction(nameof(Details), new { id = match.GroupId });
        }
        #endregion

        #region Group
        public async Task<IActionResult> Index()
        {
            var groups = _groupRepo.GetWithType();
            return View(groups);
        }

        [HttpPost]
        public async Task<IActionResult> JoinRequest(int id) //groupId
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!await _context.Groups.AnyAsync(g => g.Id == id))
            {
                return NotFound();
            }

            var alreadyRequested = await _context.GroupUsers.AnyAsync(gu => gu.GroupId == id && gu.ApplicationUserId == userId);
            if (!alreadyRequested)
            {
                var uGroup = new GroupUser
                {
                    GroupId = id,
                    ApplicationUserId = userId,
                    IsAccepted = false,
                    IsBlocked = false,
                    Points = 0
                };
                try
                {
                    await _groupRepo.JoinRequest(uGroup);
                }
                catch (System.Exception)
                {
                    throw;
                }
            }

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> AcceptRequestJoin(int id) //groupuserId
        {

            var gMember = await _groupRepo.GroupMember(id);
            if (gMember == null)
            {
                return NotFound();
            }

            gMember.IsAccepted = true;
            try
            {
                await _groupRepo.AcceptRequestJoin(gMember);
            }
            catch (System.Exception)
            {
                throw;
            }

            return RedirectToAction(nameof(Details), new { id = gMember.GroupId });
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> FireFromGroup(int id) //groupuserId
        {

            var gMember = await _groupRepo.GroupMember(id);
            if (gMember == null)
            {
                return NotFound();
            }

            gMember.IsBlocked = true;
            try
            {
                await _groupRepo.AcceptRequestJoin(gMember);
            }
            catch (System.Exception)
            {
                throw;
            }

            return RedirectToAction(nameof(Details), new { id = gMember.GroupId });
        }

        public async Task<IActionResult> Details(int id)
        {
            var model = await _groupRepo.GetByIdWithChildrens(id);
            if (model == null)
            {
                return NotFound();
            }

            return View(model);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create()
        {
            var tt = _typeRepository.GetAll().ToList();
            // ViewBag.TournamentTypes = tt.ToList();
            ViewData["TournamentTypeId"] = new SelectList(tt, "Id", "Name");
            //var model = new GroupView
            //{
            //    TournamentTypes = tt.ToList()
            //};
            return View();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create(Group model)
        {

            if (string.IsNullOrWhiteSpace(model.Name))
            {
                ModelState.AddModelError(string.Empty, "Escribe el nombre del torneo.");
            }

            if (this.ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(model.InviteCode))
                {
                    model.InviteCode = InviteCodeGenerator.Generate();
                }

                model.Active = true;
                await _groupRepo.CreateAsync(model);
                await _groupRepo.JoinRequest(new GroupUser
                {
                    GroupId = model.Id,
                    ApplicationUserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                    IsAccepted = true,
                    IsBlocked = false,
                    Points = 0
                });
                return this.RedirectToAction(nameof(Details), new { id = model.Id });
            }

            ViewData["TournamentTypeId"] = new SelectList(_typeRepository.GetAll().ToList(), "Id", "Name");
            return this.View(model);
        }
        #endregion

        #region League
        public async Task<IActionResult> DetailsLeague(int id)
        {
            var model = await _leagueRepo.GetByIdWithChildrens(id);
            if (model == null)
            {
                return NotFound();
            }

            return View(model);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateLeague(int id)//this id is from the group
        {
            var model = new League
            {
                GroupId = id
            };
            return View(model);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> CreateLeague(League model)
        {//Todo: you need to revalidate than the user than is creating be owner

            if (string.IsNullOrWhiteSpace(model.Name))
            {
                ModelState.AddModelError(string.Empty, "Escribe el nombre de la liga.");
            }

            if (this.ModelState.IsValid)
            {
                model.Id = 0;
                model.Active = true;
                await _leagueRepo.CreateAsync(model);
                return this.RedirectToAction(nameof(Details), new { id = model.GroupId });
            }

            return this.View(model);
        }
        #endregion

        #region Team
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateTeam(int id)//this id is from the group
        {
            var model = new Team
            {
                LeagueId = id
            };
            return View(model);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> CreateTeam(Team model)
        {//Todo: you need to revalidate than the user than is creating be owner

            if (string.IsNullOrWhiteSpace(model.Name))
            {
                ModelState.AddModelError(string.Empty, "Escribe el nombre del equipo.");
            }

            if (this.ModelState.IsValid)
            {
                model.Id = 0;
                model.Active = true;
                model.Initials = string.IsNullOrWhiteSpace(model.Initials)
                    ? new string(model.Name.Trim().Replace(" ", string.Empty).Take(3).ToArray()).ToUpperInvariant()
                    : model.Initials.Trim().ToUpperInvariant();
                model.MatchesPlayed = 0;
                model.MatchesWon = 0;
                model.MatchesTied = 0;
                model.MatchesLost = 0;
                model.FavorPoints = 0;
                model.AgainstPoints = 0;
                model.CumulativePoints = 0;
                model.Position = 0;
                await _teamRepository.CreateAsync(model);
                return this.RedirectToAction(nameof(DetailsLeague), new { id = model.LeagueId });
            }

            return this.View(model);
        }
        #endregion

        #region Match

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CloseMatch(int id)
        {
            var match = await _matchRepository.GetByIdWithChildrens(id);
            if (match == null)
            {
                return NotFound();
            }

            return View(match);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> CloseMatch(Match match)
        {
            var current = await _matchRepository.GetByIdWithChildrens(match.Id);
            if (current == null)
            {
                return NotFound();
            }

            if (match.LocalPoints == null || match.VisitorPoints == null || match.LocalPoints < 0 || match.VisitorPoints < 0)
            {
                ModelState.AddModelError(string.Empty, "Debes ingresar el marcador final.");
                return View(current);
            }

            var closed = await _matchClosingService.CloseMatchAsync(match.Id, match.LocalPoints.Value, match.VisitorPoints.Value);
            if (!closed)
            {
                ModelState.AddModelError(string.Empty, "El partido ya está cerrado.");
                return View(current);
            }

            TempData["Message"] = "El partido quedó cerrado y los puntos se repartieron.";
            return RedirectToAction(nameof(Details), new { id = current.GroupId });
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateMatch(int id)//this id is from the group
        {
            if (!await _context.Groups.AnyAsync(g => g.Id == id))
            {
                return NotFound();
            }

            LoadTeams(id);

            var model = new Match
            {
                GroupId = id,
                Name = "Partido",
                DateTime = DateTime.UtcNow.ToTournamentTime(_timeZone).Date.AddDays(1).AddHours(20)
            };
            return View(model);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> CreateMatch(Match model)
        {//Todo: you need to revalidate than the user than is creating be owner

            if (model.LocalId == model.VisitorId)
            {
                ModelState.AddModelError(string.Empty, "El local y el visitante deben ser equipos distintos.");
            }

            var teamsInGroup = await _context.Teams
                .CountAsync(t => (t.Id == model.LocalId || t.Id == model.VisitorId) && t.League.GroupId == model.GroupId);
            if (model.LocalId != model.VisitorId && teamsInGroup < 2)
            {
                ModelState.AddModelError(string.Empty, "Elige equipos de las ligas de este torneo.");
            }

            var kickoffUtc = model.DateTime.ToUtcFromTournamentTime(_timeZone);
            if (kickoffUtc <= DateTime.UtcNow)
            {
                ModelState.AddModelError(string.Empty, "La fecha y hora del partido deben ser futuras.");
            }

            if (this.ModelState.IsValid)
            {
                model.Id = 0;
                model.StatusId = 1;
                model.Active = true;
                model.DateTime = kickoffUtc;
                if (string.IsNullOrWhiteSpace(model.Name))
                {
                    model.Name = "Partido";
                }

                await _matchRepository.CreateAsync(model);
                TempData["Message"] = "El partido quedó creado.";
                return this.RedirectToAction(nameof(Details), new { id = model.GroupId });
            }

            LoadTeams(model.GroupId);
            return this.View(model);
        }

        private void LoadTeams(int groupId)
        {
            var teams = _teamRepository.GetAll()
                .Where(t => t.League.GroupId == groupId)
                .OrderBy(t => t.Name)
                .ToList();
            ViewData["LocalId"] = new SelectList(teams, "Id", "Name");
            ViewData["VisitorId"] = new SelectList(teams, "Id", "Name");
        }

        #endregion

    }
}