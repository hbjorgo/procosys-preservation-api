﻿using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.Procosys.Preservation.Domain;
using Equinor.Procosys.Preservation.Domain.AggregateModels.RequirementTypeAggregate;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ServiceResult;

namespace Equinor.Procosys.Preservation.Query.RequirementTypeAggregate
{
    public class GetRequirementTypeByIdQueryHandler : IRequestHandler<GetRequirementTypeByIdQuery, Result<RequirementTypeDto>>
    {
        private readonly IReadOnlyContext _context;

        public GetRequirementTypeByIdQueryHandler(IReadOnlyContext context) => _context = context;

        public async Task<Result<RequirementTypeDto>> Handle(GetRequirementTypeByIdQuery request, CancellationToken cancellationToken)
        {
            var reqType = await (from rt in _context.QuerySet<RequirementType>()
                    .Include(rt => rt.RequirementDefinitions)
                    .ThenInclude(rd => rd.Fields)
                where rt.Id == request.Id
                select rt).SingleOrDefaultAsync(cancellationToken);

            if (reqType == null)
            {
                return new NotFoundResult<RequirementTypeDto>(Strings.EntityNotFound(nameof(RequirementType), request.Id));
            }

            var dto = new RequirementTypeDto(
                reqType.Id,
                reqType.Code,
                reqType.Title,
                reqType.IsVoided,
                reqType.SortKey,
                reqType.RequirementDefinitions.Select(rd =>
                    new RequirementDefinitionDto(rd.Id,
                        rd.Title,
                        rd.IsVoided,
                        rd.DefaultIntervalWeeks,
                        rd.SortKey,
                        rd.NeedsUserInput,
                        rd.Fields.Select(f => new FieldDto(
                            f.Id,
                            f.Label,
                            f.IsVoided,
                            f.FieldType,
                            f.SortKey,
                            f.Unit,
                            f.ShowPrevious)))));

            return new SuccessResult<RequirementTypeDto>(dto);
        }
    }
}
