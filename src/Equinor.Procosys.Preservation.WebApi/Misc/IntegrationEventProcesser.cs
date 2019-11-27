using System.Threading;
using System.Threading.Tasks;
using Equinor.Procosys.Preservation.Command.IntegrationEvents;
using Equinor.Procosys.Preservation.Messaging;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Equinor.Procosys.Preservation.WebApi.Misc
{
    public class IntegrationEventProcesser : BackgroundService
    {
        private readonly IntegrationEventProcesserSettings _settings;
        private readonly ILogger<IntegrationEventProcesser> _logger;
        private readonly IEventBus _eventBus;

        public IntegrationEventProcesser(IOptions<IntegrationEventProcesserSettings> settings,
                                         ILogger<IntegrationEventProcesser> logger,
                                         IEventBus eventBus)
        {
            _settings = settings.Value;
            _logger = logger;
            _eventBus = eventBus;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _eventBus.Subscribe<TagChangedEvent, TagChangedEventHandler>();
            return Task.CompletedTask;
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _eventBus.Unsubscribe<TagChangedEvent, TagChangedEventHandler>();
            return Task.CompletedTask;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogDebug($"{nameof(IntegrationEventProcesser)} is starting.");

            stoppingToken.Register(() =>
            _logger.LogDebug($"{nameof(IntegrationEventProcesser)} is stopping."));

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogDebug($"{nameof(IntegrationEventProcesser)} is alive.");

                await Task.Delay(_settings.SleepTimeMs, stoppingToken);
            }
        }
    }
}
