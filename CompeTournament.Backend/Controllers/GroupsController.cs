using CompeTournament.Backend.Data.Entities;
using CompeTournament.Backend.Persistence.Contracts;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace CompeTournament.Backend.Controllers
{
    public class GroupsController : Controller
    {
        private readonly IGroupRepository _groupRepo;

        public GroupsController(IGroupRepository groupRepo)
        {
            _groupRepo = groupRepo;
        }
        public async Task<IActionResult> Index()
        {
            var groups =  _groupRepo.GetAll();
            return View(groups);
        }

        public async Task<IActionResult> Create()
        {
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