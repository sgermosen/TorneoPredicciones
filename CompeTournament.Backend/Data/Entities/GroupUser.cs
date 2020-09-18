using CompeTournament.Backend.Helpers;
using System.ComponentModel.DataAnnotations;

namespace CompeTournament.Backend.Data.Entities
{
    public class GroupUser : AuditEntity
    {
        public int GroupId { get; set; }
        public Group Group { get; set; }

        [StringLength(450)]
        public string ApplicationUserId { get; set; }
        public ApplicationUser ApplicationUser { get; set; }
                
        public bool IsAccepted { get; set; }

        public bool IsBlocked { get; set; }

        public int Points { get; set; }
    }
}
