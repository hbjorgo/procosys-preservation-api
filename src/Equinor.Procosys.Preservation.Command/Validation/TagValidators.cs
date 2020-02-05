using System.Linq;
using Equinor.Procosys.Preservation.Domain.AggregateModels.ProjectAggregate;

namespace Equinor.Procosys.Preservation.Command.Validation
{
    public static class TagValidators
    {
        public static ValidationResult<Tag> MustExist(this ValidationResult<Tag> result, Tag tag)
        {
            if (tag == null)
            {
                result.Errors.Add("Tag does not exist");
            }
            return result;
        }

        public static ValidationResult<Tag> MustNotBeVoided(this ValidationResult<Tag> result, Tag tag)
        {
            if (tag.IsVoided)
            {
                result.Errors.Add("Tag is voided");
            }
            return result;
        }

        public static ValidationResult<Tag> MustHavePreservationStatus(this ValidationResult<Tag> result, Tag tag, PreservationStatus status)
        {
            if (tag.Status != status)
            {
                result.Errors.Add($"Tag doesn't have the correct status ({status})");
            }
            return result;
        }

        public static ValidationResult<Tag> MustHaveNonVoidedRequirements(this ValidationResult<Tag> result, Tag tag)
        {
            if (!tag.Requirements.Any(req => req.IsVoided))
            {
                result.Errors.Add("All requirements are voided");
            }
            return result;
        }

        public static ValidationResult<Tag> MustHaveAllRequirementsReadyToBePreserved(this ValidationResult<Tag> result, Tag tag)
        {
            if (!tag.Requirements.Any(r => r.ReadyToBePreserved))
            {
                result.Errors.Add("One or more requirements are not ready to be preserved");
            }

            return result;
        }
    }
}
