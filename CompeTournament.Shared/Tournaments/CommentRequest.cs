namespace CompeTournament.Shared.Tournaments
{
    using System.ComponentModel.DataAnnotations;

    public class CommentRequest
    {
        [Required]
        [StringLength(500, MinimumLength = 1)]
        public string Body { get; set; }
    }
}
