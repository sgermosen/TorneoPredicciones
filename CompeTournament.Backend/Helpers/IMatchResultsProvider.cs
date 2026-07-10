namespace CompeTournament.Backend.Helpers
{
    using System.Threading.Tasks;

    public class MatchScore
    {
        public int LocalPoints { get; set; }

        public int VisitorPoints { get; set; }
    }

    public interface IMatchResultsProvider
    {
        Task<MatchScore> TryGetResultAsync(int matchId);
    }

    public class ManualResultsProvider : IMatchResultsProvider
    {
        public Task<MatchScore> TryGetResultAsync(int matchId) => Task.FromResult<MatchScore>(null);
    }
}
