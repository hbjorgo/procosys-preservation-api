﻿using System;
using MediatR;
using ServiceResult;

namespace Equinor.Procosys.Preservation.Command.RequirementTypeCommands.VoidRequirementType
{
    public class VoidRequirementTypeCommand : IRequest<Result<string>>
    {
        public VoidRequirementTypeCommand(int requirementTypeId, string rowVersion, Guid currentUserOid)
        {
            RequirementTypeId = requirementTypeId;
            RowVersion = rowVersion;
            CurrentUserOid = currentUserOid;
        }

        public int RequirementTypeId { get; }
        public string RowVersion { get; }
        public Guid CurrentUserOid { get; }
    }
}
