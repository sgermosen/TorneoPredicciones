namespace API.Models
{
    using System.ComponentModel.DataAnnotations.Schema;
    using Domain;

    [NotMapped]
    public class UserRequest : User
    {
        public string Password { get; set; }

        public byte[] ImageArray { get; set; }
    }
}