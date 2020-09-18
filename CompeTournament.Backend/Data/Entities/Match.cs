using CompeTournament.Backend.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CompeTournament.Backend.Data.Entities
{
    public class Match : AuditEntity, IBaseEntity
    {
        [DataType(DataType.DateTime)]
        public DateTime DateTime { get; set; }

        public int LocalId { get; set; }
        [ForeignKey("LocalId")]
        public Team Local { get; set; }

        public int VisitorId { get; set; }
        [ForeignKey("VisitorId")]
        public Team Visitor { get; set; }

        public int? LocalPoints { get; set; }

        public int? VisitorPoints { get; set; }

        public int StatusId { get; set; }
        [ForeignKey("StatusId")]
        public Status Status { get; set; }

        public int GroupId { get; set; }
        [ForeignKey("GroupId")]
        public Group Group { get; set; }
        
        public ICollection<Prediction>  Predictions { get; set; }

    }
}
