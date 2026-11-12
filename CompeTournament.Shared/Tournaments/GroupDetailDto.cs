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

        public bool CanManage { get; set; }

        public bool IsPublic { get; set; }

        public List<MatchDto> Matches { get; set; } = new List<MatchDto>();

        public List<StandingDto> Standings { get; set; } = new List<StandingDto>();

        public List<TournamentDto> Tournaments { get; set; } = new List<TournamentDto>();
    }
}
