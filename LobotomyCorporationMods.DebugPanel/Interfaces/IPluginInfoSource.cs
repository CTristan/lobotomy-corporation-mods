// SPDX-License-Identifier: MIT

#region

using System.Collections.Generic;

#endregion

namespace Hemocode.DebugPanel.Interfaces
{
    public interface IPluginInfoSource
    {
        IEnumerable<BepInExPluginInspectionInfo> GetPlugins();
    }
}
