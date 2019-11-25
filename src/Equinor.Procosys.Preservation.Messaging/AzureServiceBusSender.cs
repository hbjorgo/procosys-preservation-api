using System.Text.Json;
using System.Threading.Tasks;
using Equinor.Procosys.Preservation.Domain.IntegrationEvents;
using Microsoft.Azure.ServiceBus;

namespace Equinor.Procosys.Preservation.Messaging
{
    public class AzureServiceBusSender : IMessageSender
    {
        private readonly IQueueClient _queueClient;

        public AzureServiceBusSender(IQueueClient queueClient)
        {
            _queueClient = queueClient;
        }

        public async Task SendMessage(Domain.IntegrationEvents.Message message)
        {
            var serviceBusMessage = new Microsoft.Azure.ServiceBus.Message(JsonSerializer.SerializeToUtf8Bytes(message));
            await _queueClient.SendAsync(serviceBusMessage);
        }
    }
}
