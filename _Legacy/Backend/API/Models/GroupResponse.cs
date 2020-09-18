namespace API.Models
{
    using System.Collections.Generic;
    using Domain;

    public class GroupResponse
    {
        public int GroupId { get; set; }

        public string Name { get; set; }

        public string Requirements { get; set; }

        public int OwnerId { get; set; }

        public string Logo { get; set; }

        public int? TournamentId { get; set; }

        public   User Owner { get; set; }
        
        public   List<GroupUserResponse> GroupUsers { get; set; }

    }
}