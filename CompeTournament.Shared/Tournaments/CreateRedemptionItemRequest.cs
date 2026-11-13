namespace CompeTournament.Shared.Tournaments
{
    using System.ComponentModel.DataAnnotations;

    public class CreateRedemptionItemRequest
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(300)]
        public string Description { get; set; }

        public int CostCredits { get; set; }

        public int? Stock { get; set; }

        public bool IsDigital { get; set; }
    }
}
