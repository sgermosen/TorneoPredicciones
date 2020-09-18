namespace Backend.Models
{
    using System.ComponentModel.DataAnnotations;
    using System.Web;
    using Domain;

    public class TournamentView: Tournament
    {
        [Display(Name = "Logo")]
        public HttpPostedFileBase LogoToFile { get; set; }
    }
}