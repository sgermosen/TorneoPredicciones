using TorneoPredicciones.Classes;

namespace TorneoPredicciones.ViewModels
{
    public class ProfileViewModel
    {
        public string UserName { get; set; }

        public string Picture { get; set; }

        public string Cover { get; set; }

        public string Gender { get; set; }

        public string Link { get; set; }

        public string Locale { get; set; }

        public int AgeRange { get; set; }

        public ProfileViewModel(FacebookResponse profile)
        {
            UserName = profile.Name;
            Picture = profile.Picture.Data.Url;
            Gender = profile.Gender;
            Cover = profile.Cover.Source;
            AgeRange = profile.AgeRange.Min;
            Locale = profile.Locale;
            Link = profile.Link;
        }
    }

}
