using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CompeTournament.Backend.Data.Entities
{
    public class MatchComment
    {
        public int Id { get; set; }

        public int MatchId { get; set; }
        [ForeignKey("MatchId")]
        public Match Match { get; set; }

        [StringLength(450)]
        public string ApplicationUserId { get; set; }
        public ApplicationUser ApplicationUser { get; set; }

        [Required]
        [StringLength(500)]
        public string Body { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
