namespace CompeTournament.Shared.Tournaments
{
    using System.ComponentModel.DataAnnotations;

    public class RedeemRequest
    {
        public int RedemptionItemId { get; set; }

        [StringLength(300)]
        public string DeliveryInfo { get; set; }
    }
}
