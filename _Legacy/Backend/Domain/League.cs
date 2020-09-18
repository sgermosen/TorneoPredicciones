namespace Domain
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using Newtonsoft.Json;

    public class League
    {
        [Key]
        public int LeagueId { get; set; }

        [Required(ErrorMessage = "Este campo es requerido")]
        [StringLength(50, ErrorMessage = "La longitud maxima para el campo {0}  es: {1} y el minimo es: {2}", MinimumLength = 1)]
        [Index("League_Name_Index", IsUnique = true)]
        [Display(Name = "League")]
        public string Name { get; set; }

        [DataType(DataType.ImageUrl)]
        public string Logo { get; set; }

        [JsonIgnore]
        public virtual ICollection<Team> Teams { get; set; }
    }
}
