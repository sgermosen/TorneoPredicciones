using CompeTournament.Backend.Helpers;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CompeTournament.Backend.Data.Entities
{
    public class Notification : AuditEntity
    {
        public string ApplicationUserId { get; set; }

        [ForeignKey("ApplicationUserId")]
        public ApplicationUser ApplicationUser { get; set; }

        [StringLength(300)]
        public string Body { get; set; }

        public DateTime? ReadAt { get; set; }
    }
}
