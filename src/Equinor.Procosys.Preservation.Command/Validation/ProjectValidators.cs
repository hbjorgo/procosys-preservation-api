using Equinor.Procosys.Preservation.Domain.AggregateModels.ProjectAggregate;

namespace Equinor.Procosys.Preservation.Command.Validation
{
    public static class ProjectValidators
    {
        public static ValidationResult<Project> MustExist(this ValidationResult<Project> result, Project project)
        {
            if (project == null)
            {
                result.Errors.Add("Project does not exist");
            }
            return result;
        }

        public static ValidationResult<Project> MustNotBeClosed(this ValidationResult<Project> result, Project project)
        {
            if (project.IsClosed)
            {
                result.Errors.Add("Project is closed");
            }
            return result;
        }
    }
}
