using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Equinor.Procosys.Preservation.Messaging.Tests
{
    [TestClass]
    public class AzureServiceBusSenderTests
    {
        [TestMethod]
        public async Task MessageIsSentOnce()
        {
            var queueClient = new Mock<IQueueClient>();
            var logger = new Mock<ILogger<AzureServiceBusSender>>();
            var sender = new AzureServiceBusSender(queueClient.Object, logger.Object);
            var message = new Domain.IntegrationEvents.Message();

            await sender.SendMessageAsync(message);

            queueClient.Verify(x => x.SendAsync(It.IsAny<Message>()), Times.Once);
        }
    }
}
