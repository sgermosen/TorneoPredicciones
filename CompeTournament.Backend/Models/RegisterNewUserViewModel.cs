namespace CompeTournament.Backend.Models
{
    using System.ComponentModel.DataAnnotations;

    public class RegisterNewUserViewModel
    {
        [Required(ErrorMessage = "Escribe tu nombre.")]
        [MaxLength(50, ErrorMessage = "El nombre no puede tener más de {1} caracteres.")]
        [Display(Name = "Nombre")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Escribe tu apellido.")]
        [MaxLength(50, ErrorMessage = "El apellido no puede tener más de {1} caracteres.")]
        [Display(Name = "Apellido")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Escribe tu correo.")]
        [EmailAddress(ErrorMessage = "Escribe un correo válido.")]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "Correo")]
        public string Username { get; set; }

        [MaxLength(20, ErrorMessage = "El teléfono no puede tener más de {1} caracteres.")]
        [Display(Name = "Teléfono")]
        public string PhoneNumber { get; set; }

        //[Display(Name = "Ciudad")]
        //[Range(1, int.MaxValue, ErrorMessage = "Selecione una Ciudad")]
        //public int CityId { get; set; }

        //public IEnumerable<SelectListItem> Cities { get; set; }

        //[Display(Name = "Pais")]
        //[Range(1, int.MaxValue, ErrorMessage = "Seleccione un Pais")]
        //public int CountryId { get; set; }

        //public IEnumerable<SelectListItem> Countries { get; set; }

        [Required(ErrorMessage = "Escribe una contraseña.")]
        [MinLength(8, ErrorMessage = "La contraseña debe tener al menos {1} caracteres.")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Confirma la contraseña.")]
        [Compare("Password", ErrorMessage = "Las contraseñas no coinciden.")]
        public string Confirm { get; set; }
    }
}
