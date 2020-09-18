using CompeTournament.Backend.Data.Entities;
using System.Linq;
using System.Threading.Tasks;

namespace CompeTournament.Backend.Persistence.Contracts
{
    public interface ILeagueRepository : IRepository<League>
    {
        IQueryable<League> GetWithGroup();
        Task<League> GetByIdWithChildrens(int id);

    }
}
