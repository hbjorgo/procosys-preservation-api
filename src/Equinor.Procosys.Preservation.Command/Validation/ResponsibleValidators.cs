using Equinor.Procosys.Preservation.Domain.AggregateModels.ResponsibleAggregate;

namespace Equinor.Procosys.Preservation.Command.Validation
{
    public static class ResponsibleValidators
    {
        public static ValidationResult<Responsible> MustExist(this ValidationResult<Responsible> result, Responsible resposnible)
        {
            if (resposnible == null)
            {
                result.Errors.Add("Responsible does not exist");
            }
            return result;
        }

        public static ValidationResult<Responsible> MustNotBeVoided(this ValidationResult<Responsible> result, Responsible responsible)
        {
            if (responsible.IsVoided)
            {
                result.Errors.Add("Responsible is voided");
            }
            return result;
        }
    }
}
