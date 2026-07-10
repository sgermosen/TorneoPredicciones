namespace CompeTournament.Shared.Tournaments
{
    public class LeaderboardEntryDto
    {
        public string UserId { get; set; }

        public string FullName { get; set; }

        public int Points { get; set; }

        public bool IsAccepted { get; set; }

        public bool IsBlocked { get; set; }
    }
}
