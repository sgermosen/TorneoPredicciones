namespace CompeTournament.Shared.Auth
{
    using System.ComponentModel.DataAnnotations;

    public class DeviceRegistrationRequest
    {
        [Required]
        public string Token { get; set; }

        [StringLength(20)]
        public string Platform { get; set; }
    }
}
