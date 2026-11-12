using CompeTournament.Backend.Helpers;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CompeTournament.Backend.Data.Entities
{
    public class Tournament : AuditEntity
    {
        public bool IsPublic { get; set; }

        public short Order { get; set; }

        [DataType(DataType.ImageUrl)]
        public string PictureUrl { get; set; }

        public TournamentType TournamentType { get; set; }

        public int GroupId { get; set; }
        [ForeignKey("GroupId")]
        public Group Group { get; set; }
    }
}
