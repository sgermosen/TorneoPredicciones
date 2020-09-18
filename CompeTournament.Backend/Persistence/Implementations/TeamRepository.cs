using System.Linq;
using CompeTournament.Backend.Data;
using CompeTournament.Backend.Data.Entities;
using CompeTournament.Backend.Persistence.Contracts;
using Microsoft.EntityFrameworkCore;

namespace CompeTournament.Backend.Persistence.Implementations
{
    public class TeamRepository : Repository<Team>, ITeamRepository
    {
        private readonly ApplicationDbContext _context;
       
        public TeamRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public IQueryable<Team> GetWithLeague()
        {
            return _context.Teams
                .Include(p=>p.League)
                .AsNoTracking();
        }
    }
}
