namespace API.Models
{
    using System.Collections.Generic;
    using Domain;

    public class TournamentResponse
    {
        public int TournamentId { get; set; }

        public string Name { get; set; }

        public string Logo { get; set; }
      //  public bool IsActive { get; set; }
       // public int Order { get; set; }
        public   List<TournamentGroup> Groups { get; set; }

        public List<Date> Dates { get; set; }
    }
}