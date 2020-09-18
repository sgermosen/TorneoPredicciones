using CompeTournament.Backend.Data.Entities;
using System.Linq;
using System.Threading.Tasks;

namespace CompeTournament.Backend.Persistence.Contracts
{
    public interface ITeamRepository : IRepository<Team>
    {
        IQueryable<Team> GetWithLeague(); 
    }
}
