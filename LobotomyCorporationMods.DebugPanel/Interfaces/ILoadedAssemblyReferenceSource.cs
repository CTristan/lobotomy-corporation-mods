// SPDX-License-Identifier: MIT

#region

using System.Collections.Generic;

#endregion

namespace Hemocode.DebugPanel.Interfaces
{
    public interface ILoadedAssemblyReferenceSource
    {
        IList<LoadedAssemblyInfo> GetBaseModAssemblies();
    }
}
