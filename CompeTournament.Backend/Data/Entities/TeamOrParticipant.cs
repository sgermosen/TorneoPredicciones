using CompeTournament.Backend.Helpers;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CompeTournament.Backend.Data.Entities
{
    //Team or participant
    public class Team : AuditEntity, IBaseEntity
    {
        public string Initials { get; set; }
        [DataType(DataType.ImageUrl)]
        public string PictureUrl { get; set; }

        public int? MatchesPlayed { get; set; }

        public int? FavorPoints { get; set; }

        public int? AgainstPoints { get; set; }

        public int? MatchesWon { get; set; }

        public int? MatchesLost { get; set; }

        public int? CumulativePoints { get; set; }

        public int? MatchesTied { get; set; }

        public int? Position { get; set; }

        public int LeagueId { get; set; }
        [ForeignKey("LeagueId")]
        public League League { get; set; }
    }
}
