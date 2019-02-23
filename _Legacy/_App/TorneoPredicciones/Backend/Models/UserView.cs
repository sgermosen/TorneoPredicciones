namespace Backend.Models
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Web;
    using Domain;

    [NotMapped]
    public class UserView: User
    {
        [Display(Name = "Picture")]
        public HttpPostedFileBase PictureFile { get; set; }

        [Display(Name = "Favorite league")]
        public int FavoriteLeagueId { get; set; }

        [DataType(DataType.Password)]
        [Required(ErrorMessage = "The field {0} is required")]
        [StringLength(20, ErrorMessage = "The length for field {0} must be betwen {1} and {2} characters", MinimumLength = 6)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Required(ErrorMessage = "The field {0} is required")]
        [Compare("Password", ErrorMessage = "The password and confirm does not match")]
        [Display(Name = "Password confirm")]
        public string PasswordConfirm { get; set; }

        public List<Group> Groups { get; set; }
        //[Display(Name="Picture")]
        //public HttpPostedFileBase PictureFile { get; set; }
        //[Display(Name = "Favorite League")]
        //public int FavoriteLeagueId { get; set; }

        //[DataType(DataType.Password)]
        //[Required(ErrorMessage="El campo {0} es requerido")]
        //[StringLength(20,ErrorMessage="La longitud maxima para el campo {0} debe ser de {1}")]
        //public string Password { get; set; }

        //[DataType(DataType.Password)]
        //[Required(ErrorMessage = "El campo {0} es requerido")]
        //[Compare("Password", ErrorMessage = "La contraseña y la confirmacion deben coincidir")]
        //[Display(Name = "Password Confirm")]
        //public string PasswordConfirm { get; set; }


    }
}