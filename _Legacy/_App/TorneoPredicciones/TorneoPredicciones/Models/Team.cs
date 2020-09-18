using System.Collections.Generic;
using SQLite.Net.Attributes;
using SQLiteNetExtensions.Attributes;

namespace TorneoPredicciones.Models
{
    public class Team
    {
          [PrimaryKey]
        public int TeamId { get; set; }

        public string Name { get; set; }

        public string Logo { get; set; }

        public string Initials { get; set; }

        public int LeagueId { get; set; }

        [OneToMany(CascadeOperations = CascadeOperation.CascadeRead)]
        public List<User> Fans { get; set; }

        public string FullLogo => string.IsNullOrEmpty(Logo) ? "avatar_shield.png" : string.Format("http://torneopredicciones.azurewebsites.net/{0}", Logo.Substring(1));

        public override int GetHashCode()
        {
            return TeamId;
        }
    }

}
