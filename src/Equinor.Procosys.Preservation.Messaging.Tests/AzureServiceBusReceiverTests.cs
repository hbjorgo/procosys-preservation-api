using System.Threading.Tasks;
using Equinor.Procosys.Preservation.Domain.IntegrationEvents;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Equinor.Procosys.Preservation.Messaging.Tests
{
    [TestClass]
    public class AzureServiceBusReceiverTests
    {
        [TestMethod]
        public async Task MessageIsSentOnce()
        {
            var queueClient = new Mock<IQueueClient>();
            var logger = new Mock<ILogger<AzureServiceBusReceiver>>();
            var handler = new Mock<IMessageHandler>();
            var sender = new AzureServiceBusReceiver(queueClient.Object, handler.Object, logger.Object);

            queueClient.Object.se

            queueClient.Verify(x => x.SendAsync(It.IsAny<Message>()), Times.Once);
        }
    }
}
