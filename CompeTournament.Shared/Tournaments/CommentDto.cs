namespace CompeTournament.Shared.Tournaments
{
    using System;

    public class CommentDto
    {
        public int Id { get; set; }

        public int MatchId { get; set; }

        public string AuthorName { get; set; }

        public string Body { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
