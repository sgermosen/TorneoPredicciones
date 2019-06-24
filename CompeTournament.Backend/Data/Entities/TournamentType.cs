using CompeTournament.Backend.Helpers;
using System.Collections.Generic;

namespace CompeTournament.Backend.Data.Entities
{
    public class TournamentType : BaseEntity, IBaseEntity
    {
        public virtual ICollection<Group> Groups { get; set; }
    }
}
