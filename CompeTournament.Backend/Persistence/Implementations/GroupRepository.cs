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

        public async Task<GroupUser> GroupMember(int key)
        {
            var entity = await Context.GroupUsers.Where(p => p.Id == key)
                .Include(p => p.ApplicationUser)
                .FirstOrDefaultAsync();
            return entity;
        }

        public IQueryable<GroupUser> GroupMembers(int id)
        {
            return _context.GroupUsers.Where(p => p.GroupId == id)
                .Include(p => p.ApplicationUser)
                .Include(p => p.CreatedUser)
                .AsNoTracking();
        }

        public IQueryable<Group> GetWithType()
        {
            return _context.Groups
                .Include(p => p.TournamentType)
                .Include(p => p.GroupUsers).ThenInclude(p => p.ApplicationUser)
                .Include(p => p.CreatedUser)
                .AsNoTracking();
        }

        public async Task<Group> GetByIdWithChildrens(int key)
        {
            var entity = await Context.Groups.Where(p => p.Id == key)
                .Include(p => p.Leagues)
                .Include(p => p.GroupUsers).ThenInclude(p => p.ApplicationUser)
                .Include(p => p.Matches).ThenInclude(p => p.Local)
                .Include(p => p.Matches).ThenInclude(p => p.Visitor)
                .Include(p => p.Matches).ThenInclude(p => p.Predictions)
                .FirstOrDefaultAsync();
            return entity;
        }

        public async Task<GroupUser> JoinRequest(GroupUser entity)
        {
            await Context.GroupUsers.AddAsync(entity);
            await SaveAllAsync();
            return entity;
        }

        public async Task<GroupUser> AcceptRequestJoin(GroupUser entity)
        {
            Context.Update(entity);
            await SaveAllAsync();
            return entity;
        }

        //public async Task<bool> JoinRequest(GroupUser entity)
        //{
        //    try
        //    {
        //        await Context.GroupUsers.AddAsync(entity);
        //        await SaveAllAsync();
        //        return true;
        //    }
        //    catch (System.Exception)
        //    {

        //        return false;
        //    }


        //}
    }
}
