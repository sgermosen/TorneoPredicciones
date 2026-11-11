namespace CompeTournament.Shared.Auth
{
    using System.ComponentModel.DataAnnotations;

    public class RegisterRequest
    {
        [Required(ErrorMessage = "Escribe tu nombre.")]
        [MaxLength(50, ErrorMessage = "El nombre no puede tener más de {1} caracteres.")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Escribe tu apellido.")]
        [MaxLength(50, ErrorMessage = "El apellido no puede tener más de {1} caracteres.")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Escribe tu correo.")]
        [EmailAddress(ErrorMessage = "Escribe un correo válido.")]
        public string Email { get; set; }

        [MaxLength(20, ErrorMessage = "El teléfono no puede tener más de {1} caracteres.")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Escribe una contraseña.")]
        [MinLength(8, ErrorMessage = "La contraseña debe tener al menos {1} caracteres.")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Confirma la contraseña.")]
        [Compare(nameof(Password), ErrorMessage = "Las contraseñas no coinciden.")]
        public string Confirm { get; set; }
    }
}
