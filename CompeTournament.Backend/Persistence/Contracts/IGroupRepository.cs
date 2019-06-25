using CompeTournament.Backend.Data.Entities;
using System.Linq;
using System.Threading.Tasks;

namespace CompeTournament.Backend.Persistence.Contracts
{
    public interface IGroupRepository : IRepository<Group>
    {
        IQueryable<Group> GetWithType();
        Task<Group> GetByIdWithChildrens(int id);

    }
}
