namespace CompeTournament.Backend.Helpers
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IPushSender
    {
        Task SendAsync(IEnumerable<string> tokens, string message);
    }
}
