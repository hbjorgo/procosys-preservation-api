using Equinor.Procosys.Preservation.Domain.AggregateModels.ProjectAggregate;

namespace Equinor.Procosys.Preservation.Command.Validation
{
    public static class TagValidators
    {
        public static ValidationResult<Tag> Exists(ValidationResult<Tag> result, Tag tag)
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

        public static ValidationResult<Tag> MustHavePreservationStatus(ValidationResult<Tag> result, Tag tag, PreservationStatus status)
        {
            if (tag.Status != status)
            {
                result.Errors.Add($"Tag doesn't have the correct status ({status})");
            }
            return result;
        }
    }
}
