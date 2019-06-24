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

        public GroupsController(IGroupRepository groupRepo, ITournamentTypeRepository typeRepository)
        {
            _groupRepo = groupRepo;
            _typeRepository = typeRepository;
        }
        public async Task<IActionResult> Index()
        {
            var groups = _groupRepo.GetWithType();
            return View(groups);
        }

        public async Task<IActionResult> Details(int id)
        {
            var model = await _groupRepo.FindByIdAsync(id);

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
    }
}