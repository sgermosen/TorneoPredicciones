namespace Domain
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using Newtonsoft.Json;

    public class Status
    {
        [Key]
        public int StatusId { get; set; }

        [Required(ErrorMessage = "The field {0} is required")]
        [MaxLength(50, ErrorMessage = "The maximun length for field {0} is {1} characters")]
        [Index("Status_Name_Index", IsUnique = true)]
        [Display(Name = "Status")]
        public string Name { get; set; }

        [JsonIgnore]
        public virtual ICollection<Match> Matches { get; set; }
    }

}
