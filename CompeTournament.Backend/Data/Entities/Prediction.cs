using CompeTournament.Backend.Helpers;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CompeTournament.Backend.Data.Entities
{
    public class Prediction : AuditEntity, IBaseEntity
    {
        //[StringLength(450)]
        //public string ApplicationUserId { get; set; }
        //public ApplicationUser ApplicationUser { get; set; }

        public int MatchId { get; set; }
        [ForeignKey("MatchId")]
        public Match Match { get; set; }

        public int? LocalPoints { get; set; }

        public int? VisitorPoints { get; set; }

        public int? Position { get; set; }

        public int AdquiredPoints { get; set; }

        //public int GroupId { get; set; }
        //[ForeignKey("GroupId")]
        //public Group Group { get; set; }


    }
}
