using CompeTournament.Backend.Data.Entities;
using System.Linq;
using System.Threading.Tasks;

namespace CompeTournament.Backend.Persistence.Contracts
{
    public interface IGroupRepository : IRepository<Group>
    {
        Task<GroupUser> GroupMember(int key);

        IQueryable<GroupUser> GroupMembers(int id);

        IQueryable<Group> GetWithType();

        Task<Group> GetByIdWithChildrens(int id);

       // Task<bool> JoinRequest(GroupUser entity);

        Task<GroupUser> JoinRequest(GroupUser entity);

        Task<GroupUser> AcceptRequestJoin(GroupUser entity);
    }
}
