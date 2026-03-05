// SPDX-License-Identifier: MIT

using System.Collections.Generic;

namespace HarmonyDebugPanel.Interfaces
{
    public interface IAssemblyInspectionSource
    {
        IEnumerable<AssemblyInspectionInfo> GetAssemblies();
    }
}
