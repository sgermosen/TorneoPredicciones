namespace CompeTournament.Backend.Models
{
    using System.ComponentModel.DataAnnotations;

    public class RegisterNewUserViewModel
    {
        [Required]
        [Display(Name = "Nombre")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Apellido")]
        public string LastName { get; set; }

        [Required]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "Email")]
        public string Username { get; set; }

        [MaxLength(20, ErrorMessage = "The field {0} only can contain {1} characters length.")]
        [Display(Name = "Telefono")]
        public string PhoneNumber { get; set; }

        //[Display(Name = "Ciudad")]
        //[Range(1, int.MaxValue, ErrorMessage = "Selecione una Ciudad")]
        //public int CityId { get; set; }

        //public IEnumerable<SelectListItem> Cities { get; set; }

        //[Display(Name = "Pais")]
        //[Range(1, int.MaxValue, ErrorMessage = "Seleccione un Pais")]
        //public int CountryId { get; set; }

        //public IEnumerable<SelectListItem> Countries { get; set; }

        [Required]
        [MinLength(6)]
        public string Password { get; set; }

        [Required]
        [Compare("Password")]
        public string Confirm { get; set; }
    }
}
