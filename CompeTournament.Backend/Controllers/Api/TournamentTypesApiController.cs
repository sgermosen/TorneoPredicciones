namespace CompeTournament.Backend.Controllers.Api
{
    using CompeTournament.Backend.Data;
    using CompeTournament.Shared.Tournaments;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    [Route("api/tournament-types")]
    public class TournamentTypesApiController : ApiControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TournamentTypesApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<List<TournamentTypeDto>>> GetAll()
        {
            var types = await _context.TournamentTypes
                .OrderBy(t => t.Name)
                .Select(t => new TournamentTypeDto { Id = t.Id, Name = t.Name })
                .AsNoTracking()
                .ToListAsync();

            return Ok(types);
        }
    }
}
