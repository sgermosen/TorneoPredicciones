namespace Domain
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using Newtonsoft.Json;

    public class Tournament
    {
        [Key]
        public int TournamentId { get; set; }

        [Required(ErrorMessage = "Este campo es requerido")]
        [StringLength(50, ErrorMessage = "La longitud maxima para el campo {0} es: {1} y el minimo es: {2}", MinimumLength = 1)]
        [Index("Tournament_Name_Index", IsUnique = true)]
        [Display(Name = "Tournament")]
        public string Name { get; set; }

        [DataType(DataType.ImageUrl)]
        public string Logo { get; set; }
       
        [Display(Name = "Is Active?")]
        public bool IsActive { get; set; }

        [Display(Name = "Order")]
        public int Order { get; set; }

        [JsonIgnore]
        public virtual ICollection<TournamentGroup> Groups { get; set; }
        [JsonIgnore]
        public virtual ICollection<Date> Dates { get; set; }
        [JsonIgnore]
        public virtual ICollection<GroupUser> GroupUsers { get; set; }
    }
}
