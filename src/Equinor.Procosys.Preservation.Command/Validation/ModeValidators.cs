using Equinor.Procosys.Preservation.Domain.AggregateModels.ModeAggregate;

namespace Equinor.Procosys.Preservation.Command.Validation
{
    public static class ModeValidators
    {
        public static ValidationResult<Mode> MustExist(this ValidationResult<Mode> result, Mode mode)
        {
            if (mode == null)
            {
                result.Errors.Add("Mode does not exist");
            }
            return result;
        }

        public static ValidationResult<Mode> MustNotBeVoided(this ValidationResult<Mode> result, Mode mode)
        {
            if (mode.IsVoided)
            {
                result.Errors.Add("Mode is voided");
            }
            return result;
        }
    }
}
