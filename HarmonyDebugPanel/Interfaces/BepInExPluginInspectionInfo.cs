// SPDX-License-Identifier: MIT

using System;

namespace HarmonyDebugPanel.Interfaces
{
    public sealed class BepInExPluginInspectionInfo(string pluginId, string name, string version, AssemblyInspectionInfo assembly)
    {
        public string PluginId { get; private set; } = pluginId ?? throw new ArgumentNullException(nameof(pluginId));

        public string Name { get; private set; } = name ?? throw new ArgumentNullException(nameof(name));

        public string Version { get; private set; } = version ?? throw new ArgumentNullException(nameof(version));

        public AssemblyInspectionInfo Assembly { get; private set; } = assembly ?? throw new ArgumentNullException(nameof(assembly));
    }
}
