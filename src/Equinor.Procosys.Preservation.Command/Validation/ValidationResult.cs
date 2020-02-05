using System.Collections.Generic;
using System.Linq;

namespace Equinor.Procosys.Preservation.Command.Validation
{
    public class ValidationResult<TEntity>
    {
        public bool HasErrors => Errors.Any();
        public List<string> Errors { get; } = new List<string>();

        public string ErrorsAsString()
        {
            return string.Join(",", Errors);
        }
    }
}
