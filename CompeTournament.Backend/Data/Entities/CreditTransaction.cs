using CompeTournament.Backend.Helpers;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CompeTournament.Backend.Data.Entities
{
    public class CreditTransaction : AuditEntity
    {
        public int GroupUserId { get; set; }
        [ForeignKey("GroupUserId")]
        public GroupUser GroupUser { get; set; }

        public int Amount { get; set; }

        public int BalanceAfter { get; set; }

        [StringLength(30)]
        public string Reason { get; set; }
    }
}
