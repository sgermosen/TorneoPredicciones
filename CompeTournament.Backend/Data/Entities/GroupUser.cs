using CompeTournament.Backend.Helpers;

namespace CompeTournament.Backend.Data.Entities
{
    public class GroupUser : AuditEntity
    {
        public Group Group { get; set; }

        public ApplicationUser ApplicationUser { get; set; }
                
        public bool IsAccepted { get; set; }

        public bool IsBlocked { get; set; }

        public int Points { get; set; }
    }
}
