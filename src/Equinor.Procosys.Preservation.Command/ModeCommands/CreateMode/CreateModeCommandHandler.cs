using System.Threading;
using System.Threading.Tasks;
using Equinor.Procosys.Preservation.Domain;
using Equinor.Procosys.Preservation.Domain.AggregateModels.ModeAggregate;
using MediatR;
using ServiceResult;

namespace Equinor.Procosys.Preservation.Command.ModeCommands.CreateMode
{
    public class CreateModeCommandHandler : IRequestHandler<CreateModeCommand, Result<int>>
    {
        private readonly IModeRepository _modeRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPlantProvider _plantProvider;
        private readonly IModeService _modeService;

        public CreateModeCommandHandler(IModeRepository modeRepository, IUnitOfWork unitOfWork, IPlantProvider plantProvider, IModeService modeService)
        {
            _modeRepository = modeRepository;
            _unitOfWork = unitOfWork;
            _plantProvider = plantProvider;
            _modeService = modeService;
        }

        public async Task<Result<int>> Handle(CreateModeCommand request, CancellationToken cancellationToken)
        {
            if (await _modeService.TitleExists(request.Title))
            {
                return new InvalidResult<int>("A mode with the same title already exists.");
            }

            var newMode = new Mode(_plantProvider.Plant, request.Title);
            _modeRepository.Add(newMode);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return new SuccessResult<int>(newMode.Id);
        }
    }
}
