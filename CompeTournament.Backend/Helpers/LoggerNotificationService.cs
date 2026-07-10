namespace CompeTournament.Backend.Helpers
{
    using Microsoft.Extensions.Logging;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class LoggerNotificationService : INotificationService
    {
        private readonly ILogger<LoggerNotificationService> _logger;

        public LoggerNotificationService(ILogger<LoggerNotificationService> logger)
        {
            _logger = logger;
        }

        public Task NotifyAsync(IEnumerable<string> tags, string message)
        {
            var recipients = tags?.ToList() ?? new List<string>();
            if (recipients.Count == 0)
            {
                return Task.CompletedTask;
            }

            _logger.LogInformation("Notificacion para {Count} destinatarios: {Message}", recipients.Count, message);
            return Task.CompletedTask;
        }
    }
}
