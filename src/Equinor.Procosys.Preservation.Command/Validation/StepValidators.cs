using Equinor.Procosys.Preservation.Domain.AggregateModels.JourneyAggregate;

namespace Equinor.Procosys.Preservation.Command.Validation
{
    public static class StepValidators
    {
        public static ValidationResult<Step> MustExist(this ValidationResult<Step> result, Step step)
        {
            if (step == null)
            {
                result.Errors.Add("Step does not exist");
            }
            return result;
        }

        public static ValidationResult<Step> MustNotBeVoided(this ValidationResult<Step> result, Step step)
        {
            if (step.IsVoided)
            {
                result.Errors.Add("Step is voided");
            }
            return result;
        }
    }
}
