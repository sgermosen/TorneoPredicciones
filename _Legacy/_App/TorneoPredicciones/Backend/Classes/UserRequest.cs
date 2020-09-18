using System.ComponentModel.DataAnnotations.Schema;
using Domain;

namespace Backend.Classes
{
    [NotMapped]
    public class UserRequest : User
    {
        public string Password { get; set; }

        public byte[] ImageArray { get; set; }
    }

}