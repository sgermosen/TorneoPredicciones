namespace TorneoPredicciones.Models
{
    public class GroupUser
    {

        public int GroupUserId { get; set; }

        public int GroupId { get; set; }

        public int UserId { get; set; }
        public string Logo { get; set; }
        public bool IsAccepted { get; set; }

        public bool IsBlocked { get; set; }

        public int Points { get; set; }
        // public   Group Group { get; set; }

        public User User { get; set; }
        public string FullLogo
        {
            get {
                if (string.IsNullOrEmpty(Logo))
                {
                    return "avatar_tournament.png";
                }

                return string.Format("http://torneopredicciones.azurewebsites.net/{0}", Logo.Substring(1));
            }
        }
    }
}
