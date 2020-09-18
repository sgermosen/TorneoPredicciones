using CompeTournament.Backend.Helpers;
using System.ComponentModel.DataAnnotations;

namespace CompeTournament.Backend.Data.Entities
{
    public class Tournament : AuditEntity
    {
        public bool IsPublic { get; set; }

        public short Order { get; set; }

        [DataType(DataType.ImageUrl)]
        public string PictureUrl { get; set; }
         
        public TournamentType TournamentType { get; set; }

        public Group Group { get; set; }
    }
}
