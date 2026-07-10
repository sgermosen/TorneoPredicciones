namespace CompeTournament.Shared.Live
{
    using CompeTournament.Shared.Tournaments;
    using System.Collections.Generic;

    public class LiveMatchClosedDto
    {
        public int GroupId { get; set; }

        public int MatchId { get; set; }

        public int LocalPoints { get; set; }

        public int VisitorPoints { get; set; }

        public List<StandingDto> Standings { get; set; } = new List<StandingDto>();

        public List<LeaderboardEntryDto> Leaderboard { get; set; } = new List<LeaderboardEntryDto>();
    }
}
