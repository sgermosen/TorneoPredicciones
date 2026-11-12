namespace CompeTournament.Shared.Tournaments
{
    using System.ComponentModel.DataAnnotations;

    public class CreateLeagueRequest
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; }
    }
}
