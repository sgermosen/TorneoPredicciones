namespace CompeTournament.Shared.Tournaments
{
    public class GroupDto
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Requirements { get; set; }

        public string Logo { get; set; }

        public string TournamentTypeName { get; set; }

        public int MembersCount { get; set; }

        public bool IsMember { get; set; }

        public bool IsAccepted { get; set; }
    }
}
