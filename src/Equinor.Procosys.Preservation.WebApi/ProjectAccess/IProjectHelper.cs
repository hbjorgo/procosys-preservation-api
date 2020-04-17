﻿using System.Threading.Tasks;

namespace Equinor.Procosys.Preservation.WebApi.ProjectAccess
{
    public interface IProjectHelper
    {
        Task<string> GetProjectNameFromTagIdAsync(int tagId);
    }
}
