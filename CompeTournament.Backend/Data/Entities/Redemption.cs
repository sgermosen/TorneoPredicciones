using CompeTournament.Backend.Helpers;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CompeTournament.Backend.Data.Entities
{
    public class Redemption : AuditEntity
    {
        public int GroupUserId { get; set; }
        [ForeignKey("GroupUserId")]
        public GroupUser GroupUser { get; set; }

        public int RedemptionItemId { get; set; }
        [ForeignKey("RedemptionItemId")]
        public RedemptionItem RedemptionItem { get; set; }

        public int CostCredits { get; set; }

        [StringLength(20)]
        public string Status { get; set; }

        public string DeliveryInfo { get; set; }

        public string DeliveryReference { get; set; }
    }
}
