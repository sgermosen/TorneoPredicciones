using CompeTournament.Backend.Helpers;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CompeTournament.Backend.Data.Entities
{
    //League or Assosiation
    public class League : AuditEntity, IBaseEntity
    {
        [DataType(DataType.ImageUrl)]
        public string PictureUrl { get; set; }

        public int GroupId { get; set; }
        [ForeignKey("GroupId")]
        public Group Group { get; set; }

        public ICollection<Team> Teams { get; set; }
    }
}
