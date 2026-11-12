namespace CompeTournament.Shared.Tournaments
{
    using System.ComponentModel.DataAnnotations;

    public class CreateGroupRequest
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(500)]
        public string Requirements { get; set; }

        public int TournamentTypeId { get; set; }

        public bool? IsPublic { get; set; }
    }
}
