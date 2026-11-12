namespace CompeTournament.Shared.Tournaments
{
    using System.ComponentModel.DataAnnotations;

    public class InviteMemberRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
