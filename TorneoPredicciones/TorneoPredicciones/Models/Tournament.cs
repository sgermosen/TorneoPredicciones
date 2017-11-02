using System.Collections.Generic;

namespace TorneoPredicciones.Models
{
    public class Tournament
    {
        public int TournamentId { get; set; }

        public string Name { get; set; }

        public string Logo { get; set; }

        public List<Group> Groups { get; set; }

        public List<Date> Dates { get; set; }

        public string FullLogo => string.IsNullOrEmpty(Logo) ? "avatar_tournament.png" : string.Format("http://torneopredicciones.azurewebsites.net/{0}", Logo.Substring(1));
    }

}
