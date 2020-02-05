using Equinor.Procosys.Preservation.Domain.AggregateModels.JourneyAggregate;

namespace Equinor.Procosys.Preservation.Command.Validation
{
    public static class JourneyValidators
    {
        public static ValidationResult<Journey> MustExist(this ValidationResult<Journey> result, Journey journey)
        {
            if (journey == null)
            {
                result.Errors.Add("Journey does not exist");
            }
            return result;
        }

        public static ValidationResult<Journey> MustNotBeVoided(this ValidationResult<Journey> result, Journey journey)
        {
            if (journey.IsVoided)
            {
                result.Errors.Add("Journey is voided");
            }
            return result;
        }
    }
}
