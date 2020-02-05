using System.Collections.Generic;
using MediatR;
using ServiceResult;

namespace Equinor.Procosys.Preservation.Command.TagCommands.Preserve
{
    public class PreserveCommand : IRequest<Result<Unit>>
    {
        public PreserveCommand(int projectId, IEnumerable<int> tagIds, bool bulkPreserved)
        {
            ProjectId = projectId;
            TagIds = tagIds ?? new List<int>();
            BulkPreserved = bulkPreserved;
        }

        public int ProjectId { get; }
        public IEnumerable<int> TagIds { get; }
        public bool BulkPreserved { get; }
    }
}
