// SPDX-License-Identifier: MIT

#region

using DebugPanel.Common.Implementations;

#endregion

namespace DebugPanel.Interfaces
{
    public sealed class BepInExPluginInspectionInfo
    {
        public BepInExPluginInspectionInfo(string pluginId, string name, string version, AssemblyInspectionInfo assembly)
        {
            ThrowHelper.ThrowIfNull(pluginId);
            PluginId = pluginId;
            ThrowHelper.ThrowIfNull(name);
            Name = name;
            ThrowHelper.ThrowIfNull(version);
            Version = version;
            ThrowHelper.ThrowIfNull(assembly);
            Assembly = assembly;
        }

        public string PluginId { get; private set; }

        public string Name { get; private set; }

        public string Version { get; private set; }

        public AssemblyInspectionInfo Assembly { get; private set; }
    }
}
