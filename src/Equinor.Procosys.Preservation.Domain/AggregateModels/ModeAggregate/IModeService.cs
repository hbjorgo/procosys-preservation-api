using System.Threading.Tasks;

namespace Equinor.Procosys.Preservation.Domain.AggregateModels.ModeAggregate
{
    public interface IModeService
    {
        Task<bool> TitleExists(string title);
        Task<bool> IsModeInUse(int id);
    }
}
