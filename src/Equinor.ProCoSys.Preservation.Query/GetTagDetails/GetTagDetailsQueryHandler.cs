﻿using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Preservation.Domain;
using Equinor.ProCoSys.Preservation.Domain.AggregateModels.JourneyAggregate;
using Equinor.ProCoSys.Preservation.Domain.AggregateModels.ModeAggregate;
using Equinor.ProCoSys.Preservation.Domain.AggregateModels.ProjectAggregate;
using Equinor.ProCoSys.Preservation.Domain.AggregateModels.ResponsibleAggregate;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ServiceResult;
using PreservationAction = Equinor.ProCoSys.Preservation.Domain.AggregateModels.ProjectAggregate.Action;

namespace Equinor.ProCoSys.Preservation.Query.GetTagDetails
{
    public class GetTagDetailsQueryHandler : IRequestHandler<GetTagDetailsQuery, Result<TagDetailsDto>>
    {
        private readonly IReadOnlyContext _context;

        public GetTagDetailsQueryHandler(IReadOnlyContext context) => _context = context;

        public async Task<Result<TagDetailsDto>> Handle(GetTagDetailsQuery request, CancellationToken cancellationToken)
        {
            // Requirements and it's PreservationPeriods needs to be included so tag.IsReadyToBePreserved calculates as it should
            var tagDetails = await (from tag in _context.QuerySet<Tag>()
                                                .Include(t => t.Requirements)
                                                    .ThenInclude(r => r.PreservationPeriods)
                                    join step in _context.QuerySet<Step>() on tag.StepId equals step.Id
                                    join journey in _context.QuerySet<Journey>() on EF.Property<int>(step, "JourneyId") equals journey.Id
                                    join mode in _context.QuerySet<Mode>() on step.ModeId equals mode.Id
                                    join responsible in _context.QuerySet<Responsible>() on step.ResponsibleId equals responsible.Id
                                    let anyActions = (from a in _context.QuerySet<PreservationAction>()
                                        where EF.Property<int>(a, "TagId") == tag.Id select a.Id).Any()
                                    let anyAttachments = (from a in _context.QuerySet<TagAttachment>()
                                        where EF.Property<int>(a, "TagId") == tag.Id select a.Id).Any()
                                    where tag.Id == request.TagId
                                    select new TagDetailsDto(
                                        tag.Id,
                                        tag.TagNo,
                                        tag.Status != PreservationStatus.NotStarted || anyActions || anyAttachments,
                                        tag.IsVoided,
                                        tag.Description,
                                        tag.Status.GetDisplayValue(),
                                        new JourneyDetailsDto(journey.Id, journey.Title),
                                        new StepDetailsDto(step.Id, step.Title), 
                                        new ModeDetailsDto(mode.Id, mode.Title),
                                        new ResponsibleDetailsDto(responsible.Id, responsible.Code, responsible.Description),
                                        tag.CommPkgNo,
                                        tag.McPkgNo,
                                        tag.Calloff,
                                        tag.PurchaseOrderNo,
                                        tag.AreaCode, 
                                        tag.DisciplineCode,
                                        tag.TagType,
                                        tag.IsReadyToBePreserved(),
                                        tag.Remark,
                                        tag.StorageArea,
                                        tag.RowVersion.ConvertToString())).SingleOrDefaultAsync(cancellationToken);

            if (tagDetails == null)
            {
                return new NotFoundResult<TagDetailsDto>($"Entity with ID {request.TagId} not found");
            }

            return new SuccessResult<TagDetailsDto>(tagDetails);
        }
    }
}
