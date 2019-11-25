using System.Threading.Tasks;

namespace Equinor.Procosys.Preservation.Domain.IntegrationEvents
{
    public interface IMessageSender
    {
        Task SendMessageAsync(Message message);
    }
}
