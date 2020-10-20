﻿using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.Procosys.Preservation.Command.TagCommands.Transfer;
using Equinor.Procosys.Preservation.Domain;
using Equinor.Procosys.Preservation.Domain.AggregateModels.JourneyAggregate;
using Equinor.Procosys.Preservation.Domain.AggregateModels.ProjectAggregate;
using MediatR;
using ServiceResult;

namespace Equinor.Procosys.Preservation.Command.TagCommands.CompletePreservation
{
    public class CompletePreservationCommandHandler : IRequestHandler<CompletePreservationCommand, Result<IEnumerable<IdAndRowVersion>>>
    {
        private readonly IProjectRepository _projectRepository;
        private readonly IJourneyRepository _journeyRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CompletePreservationCommandHandler(IProjectRepository projectRepository, IJourneyRepository journeyRepository, IUnitOfWork unitOfWork)
        {
            _projectRepository = projectRepository;
            _unitOfWork = unitOfWork;
            _journeyRepository = journeyRepository;
        }

        public async Task<Result<IEnumerable<IdAndRowVersion>>> Handle(CompletePreservationCommand request, CancellationToken cancellationToken)
        {
            var tags = await _projectRepository.GetTagsByTagIdsAsync(request.Tags.Select(x => x.Id));

            var stepIds = tags.Select(t => t.StepId).Distinct();
            var journeys = await _journeyRepository.GetJourneysByStepIdsAsync(stepIds);

            foreach (var tag in tags)
            {
                var journey = journeys.Single(j => j.Steps.Any(s => s.Id == tag.StepId));
                tag.SetRowVersion(request.Tags.Single(x => x.Id == tag.Id).RowVersion);
                tag.CompletePreservation(journey);
            }
            
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            
            return new SuccessResult<IEnumerable<IdAndRowVersion>>(tags.CreateIdAndRowVersionList());
        }
    }
}
