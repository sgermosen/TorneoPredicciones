namespace API.Controllers
{
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web.Http;
    using System.Web.Http.Description;
    using Models;
    using Domain;

    public class TournamentTeamsController : ApiController
    {
        private readonly DataContext _db = new DataContext();

        // GET: api/TournamentTeams
        public IQueryable<TournamentTeam> GetTournamentTeams()
        {
            return _db.TournamentTeams;
        }

        [ResponseType(typeof(TournamentTeam))]
        public async Task<IHttpActionResult> GetTournamentTeam(int id)
        {
            var tournamentTeams = await _db.TournamentTeams.Where(tt => tt.TournamentGroupId == id).ToListAsync();
            var list = new List<TournamentTeamResponse>();
            foreach (var tournamentTeam in tournamentTeams.OrderBy(tt => tt.Position))
            {
                list.Add(new TournamentTeamResponse
                {
                    AgainstGoals = tournamentTeam.AgainstGoals,
                    FavorGoals = tournamentTeam.FavorGoals,
                    MatchesLost = tournamentTeam.MatchesLost,
                    MatchesPlayed = tournamentTeam.MatchesPlayed,
                    MatchesTied = tournamentTeam.MatchesTied,
                    MatchesWon = tournamentTeam.MatchesWon,
                    Points = tournamentTeam.Points,
                    Position = tournamentTeam.Position,
                    Team = tournamentTeam.Team,
                    TeamId = tournamentTeam.TeamId,
                    TournamentGroupId = tournamentTeam.TournamentGroupId,
                    TournamentTeamId = tournamentTeam.TournamentTeamId,
                });
            }

            return Ok(list);
        }
        // GET: api/TournamentTeams/5
        //[ResponseType(typeof(TournamentTeam))]
        //public async Task<IHttpActionResult> GetTournamentTeam(int id)
        //{
        //    var tournamentTeams = await _db.TournamentTeams.Where(tt => tt.TournamentGroupId == id).ToListAsync();
        //    //if (tournamentTeam == null)
        //    //{
        //    //    return NotFound();
        //    //}
        //    var list = new List<TournamentTeamRespose>();
        //    foreach (var tournamentTeam in tournamentTeams.OrderBy(tt => tt.Position))
        //    {
        //        list.Add(new TournamentTeamRespose
        //        {
        //            AgainstGoals = tournamentTeam.AgainstGoals,
        //            FavorGoals = tournamentTeam.FavorGoals,
        //            MatchesLost = tournamentTeam.MatchesLost,
        //            MatchesPlayed = tournamentTeam.MatchesPlayed,
        //            MatchesTied = tournamentTeam.MatchesTied,
        //            MatchesWon = tournamentTeam.MatchesWon,
        //            Points = tournamentTeam.Points,
        //            Position = tournamentTeam.Position,
        //            Team = tournamentTeam.Team,
        //            TeamId = tournamentTeam.TeamId,
        //            TournamentGroupId = tournamentTeam.TournamentGroupId,
        //            TournamentTeamId = tournamentTeam.TournamentTeamId,
        //        });
        //    }
        //    return Ok(list);
        //}

        // PUT: api/TournamentTeams/5
        //[ResponseType(typeof(void))]
        //public async Task<IHttpActionResult> PutTournamentTeam(int id, TournamentTeam tournamentTeam)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }

        //    if (id != tournamentTeam.TournamentTeamId)
        //    {
        //        return BadRequest();
        //    }

        //    _db.Entry(tournamentTeam).State = EntityState.Modified;

        //    try
        //    {
        //        await _db.SaveChangesAsync();
        //    }
        //    catch (DbUpdateConcurrencyException)
        //    {
        //        if (!TournamentTeamExists(id))
        //        {
        //            return NotFound();
        //        }
        //        else
        //        {
        //            throw;
        //        }
        //    }

        //    return StatusCode(HttpStatusCode.NoContent);
        //}

        //// POST: api/TournamentTeams
        //[ResponseType(typeof(TournamentTeam))]
        //public async Task<IHttpActionResult> PostTournamentTeam(TournamentTeam tournamentTeam)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }

        //    _db.TournamentTeams.Add(tournamentTeam);
        //    await _db.SaveChangesAsync();

        //    return CreatedAtRoute("DefaultApi", new { id = tournamentTeam.TournamentTeamId }, tournamentTeam);
        //}

        //// DELETE: api/TournamentTeams/5
        //[ResponseType(typeof(TournamentTeam))]
        //public async Task<IHttpActionResult> DeleteTournamentTeam(int id)
        //{
        //    TournamentTeam tournamentTeam = await _db.TournamentTeams.FindAsync(id);
        //    if (tournamentTeam == null)
        //    {
        //        return NotFound();
        //    }

        //    _db.TournamentTeams.Remove(tournamentTeam);
        //    await _db.SaveChangesAsync();

        //    return Ok(tournamentTeam);
        //}

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool TournamentTeamExists(int id)
        {
            return _db.TournamentTeams.Count(e => e.TournamentTeamId == id) > 0;
        }
    }
}