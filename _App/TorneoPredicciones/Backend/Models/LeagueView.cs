namespace Backend.Models
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Web;
    using Domain;

    [NotMapped]
    public class LeagueView :League
    {
        [Display(Name="Logo")]
        public HttpPostedFileBase LogoLFile { get; set; }
    }
}