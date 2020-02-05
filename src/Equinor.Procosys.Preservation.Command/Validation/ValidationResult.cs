using System.Collections.Generic;
using System.Linq;

namespace Equinor.Procosys.Preservation.Command.Validation
{
    public class ValidationResult<TEntity>
    {
        public bool HasErrors => Errors.Any();
        public List<string> Errors { get; } = new List<string>();

        public override string ToString() => string.Join(",", Errors);

        public static implicit operator bool(ValidationResult<TEntity> result) => result.HasErrors;
        public static implicit operator string(ValidationResult<TEntity> result) => result.ToString();

    }
}
