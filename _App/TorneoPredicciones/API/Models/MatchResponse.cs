namespace API.Models
{
    using Domain;
    using System;

    public class MatchResponse
    {
        public int MatchId { get; set; }

        public int DateId { get; set; }

        public DateTime DateTime { get; set; }

        public int LocalId { get; set; }

        public int VisitorId { get; set; }

        public int? LocalGoals { get; set; }

        public int? VisitorGoals { get; set; }

        public int StatusId { get; set; }

        public int TournamentGroupId { get; set; }

        public bool WasPredicted { get; set; }

        public Team Local { get; set; }

        public Team Visitor { get; set; }
    }


}