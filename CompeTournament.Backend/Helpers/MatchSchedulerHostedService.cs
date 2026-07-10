namespace CompeTournament.Backend.Helpers
{
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public class MatchSchedulerHostedService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<MatchSchedulerHostedService> _logger;

        public MatchSchedulerHostedService(
            IServiceScopeFactory scopeFactory,
            IConfiguration configuration,
            ILogger<MatchSchedulerHostedService> logger)
        {
            _scopeFactory = scopeFactory;
            _configuration = configuration;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!_configuration.GetValue("Scheduler:Enabled", true))
            {
                return;
            }

            var seconds = _configuration.GetValue("Scheduler:IntervalSeconds", 60);
            var interval = TimeSpan.FromSeconds(seconds < 5 ? 5 : seconds);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var scheduler = scope.ServiceProvider.GetRequiredService<IMatchScheduler>();
                    await scheduler.LockDueMatchesAsync();
                    await scheduler.AutoCloseWithResultsAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error ejecutando el planificador de partidos.");
                }

                await Task.Delay(interval, stoppingToken);
            }
        }
    }
}
