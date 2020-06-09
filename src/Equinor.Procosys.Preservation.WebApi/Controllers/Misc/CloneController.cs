﻿using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Equinor.Procosys.Preservation.Command.MiscCommands.Clone;
using Equinor.Procosys.Preservation.Domain;
using Equinor.Procosys.Preservation.WebApi.Misc;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceResult.ApiExtensions;

namespace Equinor.Procosys.Preservation.WebApi.Controllers.Misc
{
    [ApiController]
    [Route("Clone")]
    public class CloneController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ICurrentUserProvider _currentUserProvider;

        public CloneController(IMediator mediator, ICurrentUserProvider currentUserProvider)
        {
            _mediator = mediator;
            _currentUserProvider = currentUserProvider;
        }

        [Authorize(Roles = Permissions.LIBRARY_PRESERVATION_CREATE)]
        [HttpPut("Clone")]
        public async Task<IActionResult> Clone(
            [FromHeader(Name = PlantProvider.PlantHeader)]
            [Required]
            [StringLength(PlantEntityBase.PlantLengthMax, MinimumLength = PlantEntityBase.PlantLengthMin)]
            string targetPlant,
            [FromQuery]
            [Required]
            [StringLength(PlantEntityBase.PlantLengthMax, MinimumLength = PlantEntityBase.PlantLengthMin)]
            string sourcePlant
            )
        {
            var command = new CloneCommand(sourcePlant, targetPlant, _currentUserProvider.GetCurrentUserOid());

            var result = await _mediator.Send(command);

            return this.FromResult(result);
        }
    }
}
