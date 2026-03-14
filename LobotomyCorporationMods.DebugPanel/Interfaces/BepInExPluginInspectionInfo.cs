// SPDX-License-Identifier: MIT

#region

using LobotomyCorporationMods.Common.Extensions;
using LobotomyCorporationMods.Common.Implementations;

#endregion

namespace LobotomyCorporationMods.DebugPanel.Interfaces
{
    public sealed class BepInExPluginInspectionInfo
    {
        public BepInExPluginInspectionInfo(string pluginId, string name, string version, AssemblyInspectionInfo assembly)
        {
            PluginId = Guard.Against.Null(pluginId, nameof(pluginId));
            Name = Guard.Against.Null(name, nameof(name));
            Version = Guard.Against.Null(version, nameof(version));
            Assembly = Guard.Against.Null(assembly, nameof(assembly));
        }

        public string PluginId { get; private set; }

        public string Name { get; private set; }

        public string Version { get; private set; }

        public AssemblyInspectionInfo Assembly { get; private set; }
    }
}
