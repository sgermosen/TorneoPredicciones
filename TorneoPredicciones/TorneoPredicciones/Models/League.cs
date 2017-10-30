using System.Collections.Generic;

namespace TorneoPredicciones.Models
{
   public class League
    {

        public int LeagueId { get; set; }

        public string Name { get; set; }

        public string Logo { get; set; }

        public List<Team> Teams { get; set; }


    }
}
