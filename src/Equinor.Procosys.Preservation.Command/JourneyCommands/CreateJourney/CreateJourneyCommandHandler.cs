﻿using System.Threading;
using System.Threading.Tasks;
using Equinor.Procosys.Preservation.Domain;
using Equinor.Procosys.Preservation.Domain.AggregateModels.JourneyAggregate;
using MediatR;
using ServiceResult;

namespace Equinor.Procosys.Preservation.Command.JourneyCommands.CreateJourney
{
    public class CreateJourneyCommandHandler : IRequestHandler<CreateJourneyCommand, Result<int>>
    {
        private readonly IJourneyRepository _journeyRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPlantProvider _plantProvider;
        private readonly IJourneyService _journeyService;

        public CreateJourneyCommandHandler(IJourneyRepository journeyRepository, IUnitOfWork unitOfWork, IPlantProvider plantProvider, IJourneyService journeyService)
        {
            _journeyRepository = journeyRepository;
            _unitOfWork = unitOfWork;
            _plantProvider = plantProvider;
            _journeyService = journeyService;
        }

        public async Task<Result<int>> Handle(CreateJourneyCommand request, CancellationToken cancellationToken)
        {
            if (await _journeyService.TitleExists(request.Title))
            {
                return new InvalidResult<int>("A journey with the same title already exists.");
            }

            var newJourney = new Journey(_plantProvider.Plant, request.Title);

            _journeyRepository.Add(newJourney);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new SuccessResult<int>(newJourney.Id);
        }
    }
}
