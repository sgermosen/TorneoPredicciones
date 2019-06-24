using CompeTournament.Backend.Data.Entities;
using System.Linq;

namespace CompeTournament.Backend.Persistence.Contracts
{
    public interface IGroupRepository : IRepository<Group>
    {
        IQueryable<Group> GetWithType();

    }
}
