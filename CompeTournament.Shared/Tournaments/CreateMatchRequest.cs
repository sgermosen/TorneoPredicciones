namespace CompeTournament.Shared.Tournaments
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public class CreateMatchRequest
    {
        [StringLength(100)]
        public string Name { get; set; }

        public int LocalId { get; set; }

        public int VisitorId { get; set; }

        public DateTime DateTime { get; set; }

        public int? TournamentId { get; set; }
    }
}
