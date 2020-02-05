using Equinor.Procosys.Preservation.Domain.AggregateModels.PersonAggregate;

namespace Equinor.Procosys.Preservation.Command.Validation
{
    public static class PersonValidators
    {
        public static ValidationResult<Person> MustExist(this ValidationResult<Person> result, Person person)
        {
            if (person == null)
            {
                result.Errors.Add("Person does not exist");
            }
            return result;
        }
    }
}
