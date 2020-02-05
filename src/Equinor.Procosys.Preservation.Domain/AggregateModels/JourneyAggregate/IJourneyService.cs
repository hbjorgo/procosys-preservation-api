using System.Threading.Tasks;

namespace Equinor.Procosys.Preservation.Domain.AggregateModels.JourneyAggregate
{
    public interface IJourneyService
    {
        Task<bool> TitleExists(string title);
    }
}
