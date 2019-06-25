using CompeTournament.Backend.Data.Entities;
using CompeTournament.Backend.Persistence.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Linq;
using System.Threading.Tasks;

namespace CompeTournament.Backend.Controllers
{
    public class GroupsController : Controller
    {
        private readonly IGroupRepository _groupRepo;

        public readonly ITournamentTypeRepository _typeRepository;
        public readonly ILeagueRepository _leagueRepo;

        public GroupsController(IGroupRepository groupRepo, ITournamentTypeRepository typeRepository
            , ILeagueRepository leagueRepo)
        {
            _groupRepo = groupRepo;
            _typeRepository = typeRepository;
            _leagueRepo = leagueRepo;
        }

        #region Group
        public async Task<IActionResult> Index()
        {
            var groups = _groupRepo.GetWithType();
            return View(groups);
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


    }
}