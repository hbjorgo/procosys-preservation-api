using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Equinor.Procosys.Preservation.Domain.IntegrationEvents;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Logging;

namespace Equinor.Procosys.Preservation.Messaging
{
    public class AzureServiceBusReceiver : IMessageReceiver
    {
        private readonly QueueClient _queueClient;
        private readonly IMessageHandler _messageHandler;
        private readonly ILogger<AzureServiceBusReceiver> _logger;

        public AzureServiceBusReceiver(
            string serviceBusConnectionString,
            string queueName,
            IMessageHandler messageHandler,
            ILogger<AzureServiceBusReceiver> logger)
        {
            _queueClient = new QueueClient(serviceBusConnectionString, queueName);
            _messageHandler = messageHandler;
            _logger = logger;
        }

        public void RegisterMessageHandler()
        {
            var messageHandlerOptions = new MessageHandlerOptions(ExceptionReceivedHandler)
            {
                MaxConcurrentCalls = 1,
                AutoComplete = false
            };

            _queueClient.RegisterMessageHandler(ProcessMessageAsync, messageHandlerOptions);
        }

        private async Task ProcessMessageAsync(Microsoft.Azure.ServiceBus.Message message, CancellationToken cancellationToken)
        {
            var payload = JsonSerializer.Deserialize<Domain.IntegrationEvents.Message>(Encoding.UTF8.GetString(message.Body));
            await _messageHandler.Handle(payload);
            await _queueClient.CompleteAsync(message.SystemProperties.LockToken);
        }

        private Task ExceptionReceivedHandler(ExceptionReceivedEventArgs arg)
        {
            _logger.LogError(arg.Exception, "Message handler encountered an exception");

            var context = arg.ExceptionReceivedContext;
            _logger.LogDebug($"- Endpoint: {context.Endpoint}");
            _logger.LogDebug($"- Entity Path: {context.EntityPath}");
            _logger.LogDebug($"- Executing Action: {context.Action}");

            return Task.CompletedTask;
        }

        public Task CloseAsync()
        {
            return _queueClient.CloseAsync();
        }
    }
}
