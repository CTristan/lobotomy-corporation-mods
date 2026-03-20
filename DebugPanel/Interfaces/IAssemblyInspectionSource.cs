// SPDX-License-Identifier: MIT

#region

using System.Collections.Generic;

#endregion

namespace DebugPanel.Interfaces
{
    public interface IAssemblyInspectionSource
    {
        IEnumerable<AssemblyInspectionInfo> GetAssemblies();
    }
}
