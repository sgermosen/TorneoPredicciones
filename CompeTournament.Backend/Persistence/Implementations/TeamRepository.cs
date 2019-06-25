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
        //private readonly IUserHelper _userHelper;

        //public GroupRepository(ApplicationDbContext context, IUserHelper userHelper) : base(context)
        //{
        //    _context = context;
        //    _userHelper = userHelper;
        //}
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
