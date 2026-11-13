namespace CompeTournament.Shared.Tournaments
{
    using System.ComponentModel.DataAnnotations;

    public class DeliverRedemptionRequest
    {
        [StringLength(300)]
        public string DeliveryReference { get; set; }
    }
}
