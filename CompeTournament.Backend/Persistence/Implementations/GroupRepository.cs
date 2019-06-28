using System.Linq;
using System.Threading.Tasks;
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
                .Include(p => p.TournamentType)
                .AsNoTracking();
        }
        public async Task<Group> GetByIdWithChildrens(int key)
        {
            var entity = await Context.Groups.Where(p => p.Id == key)
                .Include(p => p.Leagues)
                  .Include(p => p.Matches).ThenInclude(p => p.Local)
                    .Include(p => p.Matches).ThenInclude(p => p.Visitor)
                .FirstOrDefaultAsync();
            return entity;
        }
    }
}
