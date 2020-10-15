﻿using System.Collections.Generic;
using System.Threading.Tasks;

namespace Equinor.Procosys.Preservation.MainApi.Permission
{
    public interface IPermissionApiService
    {
        Task<IList<string>> GetPermissionsAsync(string plantId);
        Task<IList<ProcosysProject>> GetAllOpenProjectsAsync(string plantId);
        Task<IList<string>> GetContentRestrictionsAsync(string plantId);
    }
}
