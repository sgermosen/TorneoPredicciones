using CompeTournament.Backend.Helpers;
using System.ComponentModel.DataAnnotations.Schema;

namespace CompeTournament.Backend.Data.Entities
{
    public class RedemptionItem : AuditEntity
    {
        public int GroupId { get; set; }
        [ForeignKey("GroupId")]
        public Group Group { get; set; }

        public string Description { get; set; }

        public int CostCredits { get; set; }

        public int? Stock { get; set; }

        public bool IsDigital { get; set; }
    }
}
