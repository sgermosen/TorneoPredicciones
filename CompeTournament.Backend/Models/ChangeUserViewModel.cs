namespace CompeTournament.Backend.Models
{
    using System.ComponentModel.DataAnnotations;

    public class ChangeUserViewModel
    {
        [Required]
        [Display(Name = "Nombres")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Apellidos")]
        public string LastName { get; set; }

        [MaxLength(20, ErrorMessage = "The field {0} only can contain {1} characters length.")]
        [Display(Name = "Telefono")]
        public string PhoneNumber { get; set; }

        //[Display(Name = "City")]
        //[Range(1, int.MaxValue, ErrorMessage = "You must select a city.")]
        //public int CityId { get; set; }

        //public IEnumerable<SelectListItem> Cities { get; set; }

        //[Display(Name = "Country")]
        //[Range(1, int.MaxValue, ErrorMessage = "You must select a country.")]
        //public int CountryId { get; set; }

        //public IEnumerable<SelectListItem> Countries { get; set; }
    }
}
