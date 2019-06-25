using CompeTournament.Backend.Helpers;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CompeTournament.Backend.Data.Entities
{
    //Team or participant
    public class Team : AuditEntity, IBaseEntity
    {
        public string Initials { get; set; }
        [DataType(DataType.ImageUrl)]
        public string PictureUrl { get; set; }
     
        public int LeagueId { get; set; }
        [ForeignKey("LeagueId")]
        public League League { get; set; }
    }
}
