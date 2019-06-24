using System.Linq;
using CompeTournament.Backend.Data;
using CompeTournament.Backend.Data.Entities;
using CompeTournament.Backend.Persistence.Contracts;
using Microsoft.EntityFrameworkCore;

namespace CompeTournament.Backend.Persistence.Implementations
{
    public class GroupRepository : Repository<Group>, IGroupRepository
    {
        private readonly ApplicationDbContext _context;
        //private readonly IUserHelper _userHelper;

        //public GroupRepository(ApplicationDbContext context, IUserHelper userHelper) : base(context)
        //{
        //    _context = context;
        //    _userHelper = userHelper;
        //}
        public GroupRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public IQueryable<Group> GetWithType()
        {
            return _context.Groups
                .Include(p=>p.TournamentType)
                .AsNoTracking();
        }
    }
}
