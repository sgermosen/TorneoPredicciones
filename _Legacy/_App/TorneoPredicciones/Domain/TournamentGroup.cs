namespace Domain
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using Newtonsoft.Json;

    public  class TournamentGroup
    {

        [Key]
        public int TournamentGroupId { get; set; }

        [Required(ErrorMessage = "Este campo es requerido")]
        [StringLength(50, ErrorMessage = "La longitud maxima para el campo {0}  es: {1} y el minimo es: {2}", MinimumLength = 1)]
        [Index("TournamentGroup_Name_TournamentId_Index", IsUnique = true, Order = 1)] //indice compuesto numero 1
        [Display(Name = "Group")]
        public string Name { get; set; }

        [Index("TournamentGroup_Name_TournamentId_Index", IsUnique = true, Order = 2)] //indice compuesto numero 2
        [Display(Name = "Tournament")]
        public int TournamentId { get; set; }

        [JsonIgnore]
        public virtual Tournament Tournament { get; set; }
        [JsonIgnore]
        public virtual  ICollection<TournamentTeam> TournamentTeams  { get; set; }
        [JsonIgnore]
        public virtual ICollection<Match> Matches { get; set; }
    }
}
