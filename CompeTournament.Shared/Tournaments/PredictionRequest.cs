namespace CompeTournament.Shared.Tournaments
{
    using System.ComponentModel.DataAnnotations;

    public class PredictionRequest
    {
        [Required]
        public int MatchId { get; set; }

        [Required]
        [Range(0, 999)]
        public int LocalPoints { get; set; }

        [Required]
        [Range(0, 999)]
        public int VisitorPoints { get; set; }

        public bool IsBanker { get; set; }
    }
}
