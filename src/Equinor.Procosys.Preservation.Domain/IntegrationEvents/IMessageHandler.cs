using System.Threading.Tasks;

namespace Equinor.Procosys.Preservation.Domain.IntegrationEvents
{
    public interface IMessageHandler
    {
        Task Handle(Message payload);
    }
}
