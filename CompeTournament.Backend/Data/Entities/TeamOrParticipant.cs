using CompeTournament.Backend.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CompeTournament.Backend.Data.Entities
{
    public class TeamOrParticipant : AuditEntity
    {
        public string Initials { get; set; }
        [DataType(DataType.ImageUrl)]
        public string PictureUrl { get; set; }
        public League League { get; set; }
    }
}
