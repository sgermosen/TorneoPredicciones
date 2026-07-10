namespace CompeTournament.Backend.Helpers
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface INotificationService
    {
        Task NotifyAsync(IEnumerable<string> tags, string message);
    }
}
