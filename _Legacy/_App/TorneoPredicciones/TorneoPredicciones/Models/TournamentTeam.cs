namespace TorneoPredicciones.Models
{
   public  class TournamentTeam
    {
        public int TournamentTeamId { get; set; }

        public int TournamentGroupId { get; set; }

        public int TeamId { get; set; }

        public int MatchesPlayed { get; set; }

        public int MatchesWon { get; set; }

        public int MatchesLost { get; set; }

        public int MatchesTied { get; set; }

        public int FavorGoals { get; set; }

        public int AgainstGoals { get; set; }

        public int Points { get; set; }

        public int Position { get; set; }

        public Team Team { get; set; }
        public int DifferenceGoals { get { return FavorGoals - AgainstGoals; } }
    }
}
