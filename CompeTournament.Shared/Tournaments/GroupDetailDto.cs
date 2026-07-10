namespace CompeTournament.Shared.Tournaments
{
    using System.Collections.Generic;

    public class GroupDetailDto
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Requirements { get; set; }

        public string Logo { get; set; }

        public string TournamentTypeName { get; set; }

        public bool IsMember { get; set; }

        public bool IsAccepted { get; set; }

        public List<MatchDto> Matches { get; set; } = new List<MatchDto>();

        public List<StandingDto> Standings { get; set; } = new List<StandingDto>();
    }
}
