namespace CompeTournament.Shared.Auth
{
    using System.ComponentModel.DataAnnotations;

    public class RefreshRequest
    {
        [Required]
        public string RefreshToken { get; set; }
    }
}
