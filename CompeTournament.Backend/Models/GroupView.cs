using CompeTournament.Backend.Data.Entities;
using System.Collections.Generic;

namespace CompeTournament.Backend.Models
{
    public class GroupView : Group
    {
        public bool IsAccepted { get; set; }
        public bool IsMember { get; set; }
        public bool IsOwner { get; set; }
        public bool IsBanned { get; set; }
        public List<TournamentType> TournamentTypes { get; set; }
    }
}
