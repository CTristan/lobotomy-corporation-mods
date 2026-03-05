// SPDX-License-Identifier: MIT

using System.Collections.Generic;

namespace HarmonyDebugPanel.Interfaces
{
    public interface IPluginInfoSource
    {
        IEnumerable<BepInExPluginInspectionInfo> GetPlugins();
    }
}
