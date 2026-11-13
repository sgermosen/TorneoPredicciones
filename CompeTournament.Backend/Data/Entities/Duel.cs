using CompeTournament.Backend.Helpers;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CompeTournament.Backend.Data.Entities
{
    public class Duel : AuditEntity
    {
        public int GroupId { get; set; }

        [ForeignKey("GroupId")]
        public Group Group { get; set; }

        public int MatchId { get; set; }

        [ForeignKey("MatchId")]
        public Match Match { get; set; }

        public string ChallengerUserId { get; set; }

        [ForeignKey("ChallengerUserId")]
        public ApplicationUser Challenger { get; set; }

        public string OpponentUserId { get; set; }

        [ForeignKey("OpponentUserId")]
        public ApplicationUser Opponent { get; set; }

        [StringLength(20)]
        public string Status { get; set; }
    }
}
