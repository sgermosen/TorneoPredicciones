namespace Backend.Models
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using Domain;

    [NotMapped]
    public class TournamentTeamView : TournamentTeam
    {
        [Display(Name="League")]
        public int LeagueId { get; set; }

    }
}