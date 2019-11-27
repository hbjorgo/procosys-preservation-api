using Microsoft.Extensions.Logging;

namespace Equinor.Procosys.Preservation.Messaging
{
    public class LogEventBus : IEventBus
    {
        private readonly ILogger<LogEventBus> _logger;

        public LogEventBus(ILogger<LogEventBus> logger)
        {
            _logger = logger;
        }

        public void Publish(IntegrationEvent @event)
        {
            _logger.LogInformation($"Publishing event {@event.GetType().FullName}");
        }

        public void Subscribe<T, TH>()
            where T : IntegrationEvent
            where TH : IIntegrationEventHandler<T>
        {
            _logger.LogInformation($"Subscribing to event {typeof(T).FullName} with handler {typeof(TH).FullName}");
        }

        public void Unsubscribe<T, TH>()
            where T : IntegrationEvent
            where TH : IIntegrationEventHandler<T>
        {
            _logger.LogInformation($"Unsubscribing from event {typeof(T).FullName} with handler {typeof(TH).FullName}");
        }
    }
}
