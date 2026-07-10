namespace CompeTournament.Shared.Auth
{
    using System.ComponentModel.DataAnnotations;

    public class TokenRequest
    {
        [Required]
        [EmailAddress]
        public string Username { get; set; }

        [Required]
        [MinLength(6)]
        public string Password { get; set; }
    }
}
