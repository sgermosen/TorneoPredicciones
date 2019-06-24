using CompeTournament.Backend.Helpers;
using System.ComponentModel.DataAnnotations;

namespace CompeTournament.Backend.Data.Entities
{
    //Team or participant
    public class Team : AuditEntity
    {
        public string Initials { get; set; }
        [DataType(DataType.ImageUrl)]
        public string PictureUrl { get; set; }
        public League League { get; set; }
    }
}
