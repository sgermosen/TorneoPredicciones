using System.Linq;
using System.Threading.Tasks;
using CompeTournament.Backend.Data;
using CompeTournament.Backend.Data.Entities;
using CompeTournament.Backend.Persistence.Contracts;
using Microsoft.EntityFrameworkCore;

namespace CompeTournament.Backend.Persistence.Implementations
{
    public class MatchRepository : Repository<Match>, IMatchRepository
    {
        private readonly ApplicationDbContext _context;
        
        public MatchRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public IQueryable<Match> GetWithGroup()
        {
            return _context.Matches
                .Include(p => p.Group)
                .AsNoTracking();
        }
        public async Task<Match> GetByIdWithChildrens(int key)
        {
            var entity = await Context.Matches.Where(p => p.Id == key)
                .Include(p => p.Visitor)
                 .Include(p => p.Local)
                .FirstOrDefaultAsync();
            return entity;
        }
        public async Task<Prediction> AddPrediction(Prediction entity)
        {
            await Context.Predictions.AddAsync(entity);
            await SaveAllAsync();
            return entity;
        }
    }
}
