using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CompeTournament.Backend.Data.Entities
{
    public class DeviceToken
    {
        public int Id { get; set; }

        [StringLength(450)]
        public string ApplicationUserId { get; set; }
        [ForeignKey("ApplicationUserId")]
        public ApplicationUser ApplicationUser { get; set; }

        [Required]
        public string Token { get; set; }

        [StringLength(20)]
        public string Platform { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
