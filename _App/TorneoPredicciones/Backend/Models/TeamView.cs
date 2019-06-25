namespace Backend.Models
{
    using System.Web;
    using Domain;
    using System.ComponentModel.DataAnnotations;

    public class TeamView: Team
    {
        [Display(Name = "Logo")]
        public HttpPostedFileBase LogoTFile { get; set; }
    }
}