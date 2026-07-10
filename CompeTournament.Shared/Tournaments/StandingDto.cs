namespace CompeTournament.Shared.Tournaments
{
    public class StandingDto
    {
        public int TeamId { get; set; }

        public int? Position { get; set; }

        public string Name { get; set; }

        public string Initials { get; set; }

        public string PictureUrl { get; set; }

        public int? MatchesPlayed { get; set; }

        public int? MatchesWon { get; set; }

        public int? MatchesTied { get; set; }

        public int? MatchesLost { get; set; }

        public int? FavorPoints { get; set; }

        public int? AgainstPoints { get; set; }

        public int? CumulativePoints { get; set; }
    }
}
