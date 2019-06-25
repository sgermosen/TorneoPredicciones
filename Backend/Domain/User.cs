namespace Domain
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using Newtonsoft.Json;

    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required(ErrorMessage = "The field {0} is required")]
        [MaxLength(50, ErrorMessage = "The maximun length for field {0} is {1} characters")]
        [Display(Name = "First name")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "The field {0} is required")]
        [MaxLength(50, ErrorMessage = "The maximun length for field {0} is {1} characters")]
        [Display(Name = "Last name")]
        public string LastName { get; set; }

        [Display(Name = "User type")]
        public int UserTypeId { get; set; }

        [DataType(DataType.ImageUrl)]
        public string Picture { get; set; }

        [Required(ErrorMessage = "The field {0} is required")]
        [MaxLength(100, ErrorMessage = "The maximun length for field {0} is {1} characters")]
        [DataType(DataType.EmailAddress)]
        [Index("User_Email_Index", IsUnique = true)]
        public string Email { get; set; }

        [Required(ErrorMessage = "The field {0} is required")]
        [MaxLength(20, ErrorMessage = "The maximun length for field {0} is {1} characters")]
        [Index("User_NickName_Index", IsUnique = true)]
        [Display(Name = "Nick name")]
        public string NickName { get; set; }

        [Display(Name = "Favorite team")]
        public int? FavoriteTeamId { get; set; }

        public int Points { get; set; }

        [Display(Name = "User")]
        public string FullName { get { return string.Format("{0} {1}", FirstName, LastName); } }
        [JsonIgnore]
        public virtual Team FavoriteTeam { get; set; }
        [JsonIgnore]
        public virtual UserType UserType { get; set; }
        [JsonIgnore]
        public virtual ICollection<Group> UserGroups { get; set; }
        [JsonIgnore]
        public virtual ICollection<GroupUser> GroupUsers { get; set; }
        [JsonIgnore]
        public virtual ICollection<Prediction> Predictions { get; set; }
        [JsonIgnore]
        public string PictureFullPath
        {
            get {
                if (string.IsNullOrEmpty(Picture))
                {
                    return string.Empty;
                }

                if (UserTypeId == 1)
                {
                    return string.Format("https://torneoprediccionesapi.azurewebsites.net{0}", Picture.Substring(1));
                }

                return Picture;
            }
        }
    }

}
