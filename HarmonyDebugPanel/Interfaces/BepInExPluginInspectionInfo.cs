// SPDX-License-Identifier: MIT

using System;
using HarmonyDebugPanel.Interfaces;

namespace HarmonyDebugPanel.Interfaces
{
    public sealed class BepInExPluginInspectionInfo
    {
        public BepInExPluginInspectionInfo(string pluginId, string name, string version, AssemblyInspectionInfo assembly)
        {
            PluginId = pluginId ?? throw new ArgumentNullException(nameof(pluginId));
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Version = version ?? throw new ArgumentNullException(nameof(version));
            Assembly = assembly ?? throw new ArgumentNullException(nameof(assembly));
        }

        public string PluginId { get; private set; }

        public string Name { get; private set; }

        public string Version { get; private set; }

        public AssemblyInspectionInfo Assembly { get; private set; }
    }
}
