namespace Domain
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using Newtonsoft.Json;

    public class TournamentTeam
    {
        [Key]
        public int TournamentTeamId { get; set; }

        [Index("TournamentTeam_TournamentGroupId_TeamId_Index", IsUnique = true, Order = 1)]
        [Display(Name = "Group")]
        public int TournamentGroupId { get; set; }

        [Index("TournamentTeam_TournamentGroupId_TeamId_Index", IsUnique = true, Order = 2)]
        [Display(Name = "Team")]
        public int TeamId { get; set; }

        [Display(Name = "Matches played")]
        public int MatchesPlayed { get; set; }

        [Display(Name = "Matches won")]
        public int MatchesWon { get; set; }

        [Display(Name = "Matches lost")]
        public int MatchesLost { get; set; }

        [Display(Name = "Matches tied")]
        public int MatchesTied { get; set; }

        [Display(Name = "Favor goals")]
        public int FavorGoals { get; set; }

        [Display(Name = "Against goals")]
        public int AgainstGoals { get; set; }

        public int Points { get; set; }

        public int Position { get; set; }

        [JsonIgnore]
        public virtual TournamentGroup TournamentGroup { get; set; }
        [JsonIgnore]
        public virtual Team Team { get; set; }
    }

}
