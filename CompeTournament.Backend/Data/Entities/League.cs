using CompeTournament.Backend.Helpers;
using System.ComponentModel.DataAnnotations;

namespace CompeTournament.Backend.Data.Entities
{
    //League or Assosiation
    public class League : AuditEntity
    {
        [DataType(DataType.ImageUrl)]
        public string PictureUrl { get; set; }

        public Group Group { get; set; }

    }
}
