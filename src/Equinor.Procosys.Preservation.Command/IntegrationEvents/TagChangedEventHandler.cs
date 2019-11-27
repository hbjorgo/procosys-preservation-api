using System.Threading.Tasks;
using Equinor.Procosys.Preservation.Messaging;

namespace Equinor.Procosys.Preservation.Command.IntegrationEvents
{
    public class TagChangedEventHandler : IIntegrationEventHandler<TagChangedEvent>
    {
        public Task Handle(TagChangedEvent @event)
        {
            throw new System.NotImplementedException();
        }
    }
}
