namespace CompeTournament.Shared.Tournaments
{
    using System.ComponentModel.DataAnnotations;

    public class CreateDuelRequest
    {
        [Required]
        public string OpponentUserId { get; set; }

        public int MatchId { get; set; }
    }
}
