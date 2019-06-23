namespace CompeTournament.Backend.Models
{
    using System.ComponentModel.DataAnnotations;

    public class ChangePasswordViewModel
    {
        [Required]
        [Display(Name = "Contraseña Actual?")]
        public string OldPassword { get; set; }

        [Required]
        [Display(Name = "Nueva contraseña")]
        public string NewPassword { get; set; }

        [Required]
        [Compare("NewPassword")]
        [Display(Name = "Confirmacion de Contraseña")]
        public string Confirm { get; set; }
    }
}
