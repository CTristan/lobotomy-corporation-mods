// SPDX-License-Identifier: MIT

#region

using System.Collections.Generic;

#endregion

namespace LobotomyCorporationMods.DebugPanel.Interfaces
{
    public interface IAssemblyInspectionSource
    {
        IEnumerable<AssemblyInspectionInfo> GetAssemblies();
    }
}
