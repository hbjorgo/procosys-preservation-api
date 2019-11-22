using System;
using System.Threading.Tasks;
using Equinor.Procosys.Preservation.Domain.IntegrationEvents;

namespace Equinor.Procosys.Preservation.Command.IntegrationEventHandlers
{
    public class MessageHandler : IMessageHandler
    {
        public Task Handle(Message payload)
        {
            throw new NotImplementedException();
        }
    }
}
