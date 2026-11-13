namespace CompeTournament.Backend.Helpers
{
    using CompeTournament.Backend.Data;
    using CompeTournament.Backend.Data.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class DbNotificationService : INotificationService
    {
        private const string UserPrefix = "userId:";

        private readonly ApplicationDbContext _context;
        private readonly IPushSender _pushSender;
        private readonly ILogger<DbNotificationService> _logger;

        public DbNotificationService(ApplicationDbContext context, IPushSender pushSender, ILogger<DbNotificationService> logger)
        {
            _context = context;
            _pushSender = pushSender;
            _logger = logger;
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

            try
            {
                var tokens = await _context.DeviceTokens
                    .Where(d => recipients.Contains(d.ApplicationUserId))
                    .Select(d => d.Token)
                    .ToListAsync();

                if (tokens.Count > 0)
                {
                    await _pushSender.SendAsync(tokens, message);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "No se pudo enviar el push; la notificacion en la bandeja si quedo guardada.");
            }
        }
    }
}
