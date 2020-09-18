using CompeTournament.Backend.Data.Entities;
using System.Linq;
using System.Threading.Tasks;

namespace CompeTournament.Backend.Persistence.Contracts
{
    public interface IMatchRepository : IRepository<Match>
    {
        IQueryable<Match> GetWithGroup();
        Task<Match> GetByIdWithChildrens(int id);
        Task<Prediction> AddPrediction(Prediction entity);

    }
}
