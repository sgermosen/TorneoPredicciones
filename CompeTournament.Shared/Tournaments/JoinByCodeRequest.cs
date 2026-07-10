namespace CompeTournament.Shared.Tournaments
{
    using System.ComponentModel.DataAnnotations;

    public class JoinByCodeRequest
    {
        [Required]
        [StringLength(12, MinimumLength = 4)]
        public string Code { get; set; }
    }
}
