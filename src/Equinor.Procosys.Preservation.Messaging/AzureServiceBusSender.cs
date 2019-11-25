using System.Text.Json;
using System.Threading.Tasks;
using Equinor.Procosys.Preservation.Domain.IntegrationEvents;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Logging;

namespace Equinor.Procosys.Preservation.Messaging
{
    public class AzureServiceBusSender : IMessageSender
    {
        private readonly IQueueClient _queueClient;
        private readonly ILogger<AzureServiceBusSender> _logger;

        public AzureServiceBusSender(IQueueClient queueClient, ILogger<AzureServiceBusSender> logger)
        {
            _queueClient = queueClient;
            _logger = logger;
        }

        public async Task SendMessageAsync(Domain.IntegrationEvents.Message message)
        {
            _logger.LogDebug($"Sending message: {message}");
            var serviceBusMessage = new Microsoft.Azure.ServiceBus.Message(JsonSerializer.SerializeToUtf8Bytes(message));
            await _queueClient.SendAsync(serviceBusMessage);
        }
    }
}
