﻿using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Preservation.Domain;
using Equinor.ProCoSys.Preservation.Domain.AggregateModels.ProjectAggregate;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.Preservation.Command.TagCommands.UpdateTag
{

    public class UpdateTagCommandHandler : IRequestHandler<UpdateTagCommand, Result<string>>
    {
        private readonly IProjectRepository _projectRepository;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateTagCommandHandler(IProjectRepository projectRepository, IUnitOfWork unitOfWork)
        {
            _projectRepository = projectRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<string>> Handle(UpdateTagCommand request, CancellationToken cancellationToken)
        {
            var tag = await _projectRepository.GetTagOnlyByTagIdAsync(request.TagId);

            tag.StorageArea = request.StorageArea;
            tag.Remark = request.Remark;
            tag.SetRowVersion(request.RowVersion);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new SuccessResult<string>(tag.RowVersion.ConvertToString());
        }
    }
}
