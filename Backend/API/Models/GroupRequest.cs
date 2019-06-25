namespace API.Models
{
    using System.ComponentModel.DataAnnotations.Schema;
    using Domain;

    [NotMapped]
    public class GroupRequest : Group
    {
        public byte[] ImageArray { get; set; }
    }
}