namespace CompeTournament.Shared.Tournaments
{
    using System;

    public class RedemptionDto
    {
        public int Id { get; set; }

        public int RedemptionItemId { get; set; }

        public string ItemName { get; set; }

        public bool IsDigital { get; set; }

        public int CostCredits { get; set; }

        public string Status { get; set; }

        public string DeliveryInfo { get; set; }

        public string DeliveryReference { get; set; }

        public string UserId { get; set; }

        public string UserFullName { get; set; }

        public DateTime? CreatedAt { get; set; }
    }
}
