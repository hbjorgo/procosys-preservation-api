using Microsoft.Extensions.Logging;

namespace Equinor.Procosys.Preservation.Messaging
{
    public class PublishToLogEventBus : IEventBus
    {
        private readonly ILogger<PublishToLogEventBus> _logger;

        public PublishToLogEventBus(ILogger<PublishToLogEventBus> logger)
        {
            _logger = logger;
        }

        public void Publish(IntegrationEvent @event)
        {
            _logger.LogInformation(@event.ToString());
        }

        public void Subscribe<T, TH>()
            where T : IntegrationEvent
            where TH : IIntegrationEventHandler<T>
        {
        }

        public void Unsubscribe<T, TH>()
            where T : IntegrationEvent
            where TH : IIntegrationEventHandler<T>
        {
        }
    }
}
