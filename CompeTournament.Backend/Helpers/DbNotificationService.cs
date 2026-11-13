namespace CompeTournament.Backend.Helpers
{
    using CompeTournament.Backend.Data;
    using CompeTournament.Backend.Data.Entities;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class DbNotificationService : INotificationService
    {
        private const string UserPrefix = "userId:";

        private readonly ApplicationDbContext _context;

        public DbNotificationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task NotifyAsync(IEnumerable<string> tags, string message)
        {
            var recipients = (tags ?? Enumerable.Empty<string>())
                .Where(t => !string.IsNullOrWhiteSpace(t) && t.StartsWith(UserPrefix))
                .Select(t => t.Substring(UserPrefix.Length))
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct()
                .ToList();

            if (recipients.Count == 0)
            {
                return;
            }

            foreach (var userId in recipients)
            {
                _context.Notifications.Add(new Notification
                {
                    ApplicationUserId = userId,
                    Body = message
                });
            }

            await _context.SaveChangesAsync();
        }
    }
}
