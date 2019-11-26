using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Equinor.Procosys.Preservation.WebApi.Misc
{
    public class IntegrationEventProcesser : BackgroundService
    {
        private readonly IntegrationEventsBackgroundSettings _settings;
        private readonly ILogger<IntegrationEventProcesser> _logger;

        public IntegrationEventProcesser(IOptions<IntegrationEventsBackgroundSettings> settings,
                                         ILogger<IntegrationEventProcesser> logger)
        {
            _settings = settings.Value;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogDebug($"{nameof(IntegrationEventProcesser)} is starting.");

            stoppingToken.Register(() =>
            _logger.LogDebug($"{nameof(IntegrationEventProcesser)} is stopping."));

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogDebug($"{nameof(IntegrationEventProcesser)} is alive.");

                await Task.Delay(_settings.SleepTime, stoppingToken);
            }
        }
    }
}
