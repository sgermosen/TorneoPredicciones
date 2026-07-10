namespace CompeTournament.Shared.Tournaments
{
    public class InsightsDto
    {
        public int TotalResolved { get; set; }

        public int ExactHits { get; set; }

        public int OutcomeHits { get; set; }

        public int Misses { get; set; }

        public double Accuracy { get; set; }

        public int CurrentStreak { get; set; }

        public int BestStreak { get; set; }

        public int TotalPoints { get; set; }
    }
}
