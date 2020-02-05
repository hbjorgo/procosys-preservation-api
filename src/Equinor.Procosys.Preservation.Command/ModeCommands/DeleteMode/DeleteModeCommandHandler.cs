using System.Threading;
using System.Threading.Tasks;
using Equinor.Procosys.Preservation.Command.Validation;
using Equinor.Procosys.Preservation.Domain;
using Equinor.Procosys.Preservation.Domain.AggregateModels.ModeAggregate;
using MediatR;
using ServiceResult;

namespace Equinor.Procosys.Preservation.Command.ModeCommands.DeleteMode
{
    public class DeleteModeCommandHandler : IRequestHandler<DeleteModeCommand, Result<Unit>>
    {
        private readonly IModeRepository _modeRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IModeService _modeService;

        public DeleteModeCommandHandler(IModeRepository modeRepository, IUnitOfWork unitOfWork, IModeService modeService)
        {
            _modeRepository = modeRepository;
            _unitOfWork = unitOfWork;
            _modeService = modeService;
        }

        public async Task<Result<Unit>> Handle(DeleteModeCommand request, CancellationToken cancellationToken)
        {
            var mode = await _modeRepository.GetByIdAsync(request.ModeId);
            var modeValidation = new ValidationResult<Mode>()
                .MustExist(mode)
                .MustNotBeVoided(mode);
            if (modeValidation)
            {
                return new InvalidResult<Unit>(modeValidation);
            }
            if (await _modeService.IsModeInUse(mode.Id))
            {
                return new InvalidResult<Unit>("Mode is in use and cannot be deleted");
            }

            _modeRepository.Remove(mode);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return new SuccessResult<Unit>(Unit.Value);
        }
    }
}
