using System.Threading.Tasks;

namespace Equinor.Procosys.Preservation.Messaging
{
    public interface IIntegrationEventHandler<TIntegrationEvent> where TIntegrationEvent : IntegrationEvent
    {
        Task Handle(TIntegrationEvent @event);
    }
}
