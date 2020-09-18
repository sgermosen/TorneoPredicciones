namespace Domain
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using Newtonsoft.Json;

    public class Group
    {
        [Key]
        public int GroupId { get; set; }

        [Required(ErrorMessage = "The field {0} is required")]
        [MaxLength(50, ErrorMessage = "The maximun length for field {0} is {1} characters")]
        [Index("Group_Name_Index", IsUnique = true)]
        [Display(Name = "Group")]
        public string Name { get; set; }

        [Required(ErrorMessage = "The field {0} is required")]
        [MaxLength(500, ErrorMessage = "The maximun length for field {0} is {1} characters")]
        public string Requirements { get; set; }

        [Display(Name = "User")]
        public int OwnerId { get; set; }

        [DataType(DataType.ImageUrl)]
        public string Logo { get; set; }

        public int? TournamentId { get; set; }

        [JsonIgnore]
        public virtual User Owner { get; set; }

        [JsonIgnore]
        public virtual Tournament Tournament { get; set; }

        [JsonIgnore]
        public virtual ICollection<GroupUser> GroupUsers { get; set; }

        [NotMapped]
        public byte[] ImageArray { get;  set; }

        [JsonIgnore]
        public string PictureFullPath
        {
            get {
                if (string.IsNullOrEmpty(Logo))
                {
                    return string.Empty;
                }

              //  if (UserTypeId == 1)
              //  {
                    return string.Format("https://torneoprediccionesapi.azurewebsites.net{0}", Logo.Substring(1));
              //  }

               // return Picture;
            }
        }
    }

}
