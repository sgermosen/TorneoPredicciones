using CompeTournament.Backend.Data.Entities;
using CompeTournament.Backend.Persistence.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CompeTournament.Backend.Controllers
{
    public class GroupsController : Controller
    {
        private readonly IGroupRepository _groupRepo;

        public readonly ITournamentTypeRepository _typeRepository;
        public readonly ILeagueRepository _leagueRepo;
        private readonly ITeamRepository _teamRepository;
        private readonly IMatchRepository _matchRepository;

        public GroupsController(IGroupRepository groupRepo, ITournamentTypeRepository typeRepository
            , ILeagueRepository leagueRepo, ITeamRepository teamRepository, IMatchRepository matchRepository)
        {
            _groupRepo = groupRepo;
            _typeRepository = typeRepository;
            _leagueRepo = leagueRepo;
            _teamRepository = teamRepository;
            _matchRepository = matchRepository;
        }

        #region Group
        public async Task<IActionResult> Index()
        {
            var groups = _groupRepo.GetWithType();
            return View(groups);
        }

        public async Task<IActionResult> JoinRequest(int id) //groupId
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
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

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> AcceptRequestJoin(int id) //groupuserId
        {

            var gMember = await _groupRepo.GroupMember(id);
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


        public async Task<IActionResult> Details(int id)
        {
            var model = await _groupRepo.GetByIdWithChildrens(id);

            return View(model);
        }

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

        [HttpPost]
        public async Task<IActionResult> Create(Group model)
        {

            if (this.ModelState.IsValid)
            {
                await _groupRepo.CreateAsync(model);
                return this.RedirectToAction(nameof(Index));
            }

            return this.View(model);
        }
        #endregion

        #region League
        public async Task<IActionResult> DetailsLeague(int id)
        {
            var model = await _leagueRepo.GetByIdWithChildrens(id);

            return View(model);
        }

        public async Task<IActionResult> CreateLeague(int id)//this id is from the group
        {
            var model = new League
            {
                GroupId = id
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> CreateLeague(League model)
        {//Todo: you need to revalidate than the user than is creating be owner

            if (this.ModelState.IsValid)
            {
                model.Id = 0;
                await _leagueRepo.CreateAsync(model);
                return this.RedirectToAction(nameof(Details), new { id = model.GroupId });
            }

            return this.View(model);
        }
        #endregion

        #region Team
        public async Task<IActionResult> CreateTeam(int id)//this id is from the group
        {
            var model = new Team
            {
                LeagueId = id
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> CreateTeam(Team model)
        {//Todo: you need to revalidate than the user than is creating be owner

            if (this.ModelState.IsValid)
            {
                model.Id = 0;
                await _teamRepository.CreateAsync(model);
                return this.RedirectToAction(nameof(DetailsLeague), new { id = model.LeagueId });
            }

            return this.View(model);
        }
        #endregion

        #region Match

        public async Task<IActionResult> DetailsMatch(int id)
        {
            var model = await _matchRepository.GetByIdWithChildrens(id);

            return View(model);
        }

        public async Task<IActionResult> CreateMatch(int id)//this id is from the group
        {
            var tt = _teamRepository.GetAll().ToList();//TODO: need to be filtered
            // ViewBag.TournamentTypes = tt.ToList();
            ViewData["LocalId"] = new SelectList(tt, "Id", "Name");
            ViewData["VisitorId"] = new SelectList(tt, "Id", "Name");

            var model = new Match
            {
                GroupId = id
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> CreateMatch(Match model)
        {//Todo: you need to revalidate than the user than is creating be owner

            if (this.ModelState.IsValid)
            {
                model.Id = 0;
                model.StatusId = 1;
                await _matchRepository.CreateAsync(model);
                return this.RedirectToAction(nameof(Details), new { id = model.GroupId });
            }

            return this.View(model);
        }

        #endregion

    }
}