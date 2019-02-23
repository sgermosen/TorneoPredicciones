namespace Domain
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using Newtonsoft.Json;

    public  class Team
    {
        [Key]
        public int TeamId { get; set; }

        [Required(ErrorMessage = "Este campo es requerido")]
        [StringLength(50, ErrorMessage = "La longitud maxima para el campo {0}  es: {1} y el minimo es: {2}", MinimumLength = 1)]
        [Index("Team_Name_LeagueId_Index", IsUnique = true,Order =1)] //indice compuesto numero 2
        [Display(Name = "Team")]
        public string Name { get; set; }

        [DataType(DataType.ImageUrl)]
        public string Logo { get; set; }

        [Required(ErrorMessage = "Este campo es requerido")]
        [StringLength(3, ErrorMessage = "La longitud para el campo {0}  debe ser {1} caracteres", MinimumLength = 3)]
        [Index("Team_Initials_LeagueId_Index", IsUnique = true, Order = 1)] //indice compuesto numero 1
        public string Initials { get; set; }

        [Index("Team_Name_LeagueId_Index", IsUnique = true, Order = 2)] //indice compuesto numero 2
        [Index("Team_Initials_LeagueId_Index", IsUnique = true, Order = 2)] //indice compuesto numero 2
        [Display(Name = "League")]
        public int LeagueId { get; set; }

        [JsonIgnore]
        public virtual League League { get; set; }
        [JsonIgnore]
        public virtual ICollection<TournamentTeam> TournamentTeams { get; set; }
        [JsonIgnore]
        public virtual ICollection<User> Fans { get; set; }
        [JsonIgnore]
        public virtual ICollection<Match> Locals { get; set; }
        [JsonIgnore]
        public virtual ICollection<Match> Visitors { get; set; }
    }
}
