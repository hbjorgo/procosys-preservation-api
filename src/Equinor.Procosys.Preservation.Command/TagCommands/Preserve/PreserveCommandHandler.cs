using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.Procosys.Preservation.Command.Validation;
using Equinor.Procosys.Preservation.Domain;
using Equinor.Procosys.Preservation.Domain.AggregateModels.PersonAggregate;
using Equinor.Procosys.Preservation.Domain.AggregateModels.ProjectAggregate;
using MediatR;
using ServiceResult;

namespace Equinor.Procosys.Preservation.Command.TagCommands.Preserve
{
    public class PreserveCommandHandler : IRequestHandler<PreserveCommand, Result<Unit>>
    {
        private readonly IProjectRepository _projectRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserProvider _currentUserProvider;
        private readonly ITimeService _timeService;

        public PreserveCommandHandler(
            IProjectRepository projectRepository,
            ITimeService timeService,
            IUnitOfWork unitOfWork,
            ICurrentUserProvider currentUserProvider)
        {
            _projectRepository = projectRepository;
            _timeService = timeService;
            _unitOfWork = unitOfWork;
            _currentUserProvider = currentUserProvider;
        }

        public async Task<Result<Unit>> Handle(PreserveCommand request, CancellationToken cancellationToken)
        {
            var project = await _projectRepository.GetByIdAsync(request.ProjectId);
            var projectValidation = new ValidationResult<Project>()
                .MustExist(project)
                .MustNotBeClosed(project);
            if (projectValidation)
            {
                return new InvalidResult<Unit>(projectValidation);
            }

            var currentUser = await _currentUserProvider.GetCurrentUserAsync();
            var userValidation = new ValidationResult<Person>()
                .MustExist(currentUser);
            if (userValidation)
            {
                return new InvalidResult<Unit>(userValidation);
            }

            foreach (var tagId in request.TagIds)
            {
                var tag = project.Tags.SingleOrDefault(tag => tag.Id == tagId);
                var tagValidation = new ValidationResult<Tag>()
                    .MustExist(tag)
                    .MustNotBeVoided(tag)
                    .MustHavePreservationStatus(tag, PreservationStatus.Active)
                    .MustHaveNonVoidedRequirements(tag)
                    // HaveExistingRequirementDefinitions
                    .MustHaveAllRequirementsReadyToBePreserved(tag);

                if (tagValidation)
                {
                    return new InvalidResult<Unit>(tagValidation);
                }

                tag.Preserve(_timeService.GetCurrentTimeUtc(), currentUser, request.BulkPreserved);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            
            return new SuccessResult<Unit>(Unit.Value);
        }
    }
}
