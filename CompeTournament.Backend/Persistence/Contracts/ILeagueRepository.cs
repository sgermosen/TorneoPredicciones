using CompeTournament.Backend.Data.Entities;
using System.Linq;

namespace CompeTournament.Backend.Persistence.Contracts
{
    public interface ILeagueRepository : IRepository<League>
    {
        IQueryable<League> GetWithGroup();

    }
}
