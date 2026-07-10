namespace CompeTournament.Shared.Tournaments
{
    public class PredictionDto
    {
        public int Id { get; set; }

        public int MatchId { get; set; }

        public int? LocalPoints { get; set; }

        public int? VisitorPoints { get; set; }

        public int AdquiredPoints { get; set; }
    }
}
