using CompeTournament.Backend.Data.Entities;
using System.Collections.Generic;

namespace CompeTournament.Backend.Models
{
    public class GroupView : Group
    {
        public List<TournamentType> TournamentTypes { get; set; }
    }
}
