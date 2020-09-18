using CompeTournament.Backend.Data;
using CompeTournament.Backend.Data.Entities;
using CompeTournament.Backend.Persistence.Contracts;

namespace CompeTournament.Backend.Persistence.Implementations
{
    public class TournamentTypeRepository : Repository<TournamentType>, ITournamentTypeRepository
    {
        private readonly ApplicationDbContext _context;

        public TournamentTypeRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

    }
}
