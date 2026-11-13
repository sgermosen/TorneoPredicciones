namespace CompeTournament.Shared.Tournaments
{
    public class RedemptionItemDto
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public int CostCredits { get; set; }

        public int? Stock { get; set; }

        public bool IsDigital { get; set; }

        public bool Active { get; set; }
    }
}
