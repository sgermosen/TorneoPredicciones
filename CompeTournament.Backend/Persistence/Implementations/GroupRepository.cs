using CompeTournament.Backend.Data;
using CompeTournament.Backend.Data.Entities;
using CompeTournament.Backend.Helpers;
using CompeTournament.Backend.Persistence.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CompeTournament.Backend.Persistence.Implementations
{
    public class GroupRepository : Repository<Group>, IGroupRepository
    {
        //private readonly ApplicationDbContext _context;
        //private readonly IUserHelper _userHelper;

        //public GroupRepository(ApplicationDbContext context, IUserHelper userHelper) : base(context)
        //{
        //    _context = context;
        //    _userHelper = userHelper;
        //}
        public GroupRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
