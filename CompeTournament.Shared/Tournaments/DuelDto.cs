namespace CompeTournament.Shared.Tournaments
{
    public class DuelDto
    {
        public int Id { get; set; }

        public int MatchId { get; set; }

        public string MatchLabel { get; set; }

        public string ChallengerUserId { get; set; }

        public string ChallengerName { get; set; }

        public string OpponentUserId { get; set; }

        public string OpponentName { get; set; }

        public string Status { get; set; }

        public bool MatchClosed { get; set; }

        public int? ChallengerPoints { get; set; }

        public int? OpponentPoints { get; set; }

        public string WinnerName { get; set; }

        public string StatusLabel { get; set; }

        public bool CanRespond { get; set; }
    }
}
