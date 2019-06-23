using CompeTournament.Backend.Data.Entities;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace CompeTournament.Backend.Data
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [MaxLength(50)]
        public string Name { get; set; }

        [Required]
        [MaxLength(50)]
        public string Lastname { get; set; }
           
        public string FullName => $"{Name} {Lastname}";

        public UserType UserType { get; set; }
        
    }
}