namespace Backend.Models
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Web;
    using Domain;

    [NotMapped]
    public class GroupView : Group
    {
        [Display(Name = "Logo")]
        public HttpPostedFileBase LogoGFile { get; set; }
    }
}