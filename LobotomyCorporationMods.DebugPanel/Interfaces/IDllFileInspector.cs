// SPDX-License-Identifier: MIT

#region

using System.Collections.Generic;

#endregion

namespace Hemocode.DebugPanel.Interfaces
{
    public interface IDllFileInspector
    {
        bool IsDeepInspectionAvailable { get; }

        IList<string> GetAssemblyReferences(string dllPath);
    }
}
