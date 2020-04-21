﻿using Equinor.Procosys.Preservation.Domain;
using MediatR;
using ServiceResult;

namespace Equinor.Procosys.Preservation.Command.JourneyCommands.UpdateJourney
{
    public class UpdateJourneyCommand : IRequest<Result<Unit>>, IJourneyRequest
    {
        public UpdateJourneyCommand(int journeyId, string title)
        {
            JourneyId = journeyId;
            Title = title;
        }
        
        public int JourneyId { get; set;}
        public string Title { get; set; }

    }
}
