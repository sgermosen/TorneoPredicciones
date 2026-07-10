namespace CompeTournament.Backend.Helpers
{
    using CompeTournament.Shared.Tournaments;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class RecapContext
    {
        public string GroupName { get; set; }

        public IReadOnlyList<LeaderboardEntryDto> Leaderboard { get; set; } = new List<LeaderboardEntryDto>();

        public string LastMatchSummary { get; set; }
    }

    public interface IJornadaRecapGenerator
    {
        Task<string> GenerateAsync(RecapContext context);
    }
}
