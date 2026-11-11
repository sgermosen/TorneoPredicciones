namespace CompeTournament.Backend.Helpers
{
    public interface IMailHelper
    {
        bool SendMail(string to, string subject, string body);
    }
}
