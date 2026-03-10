// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using BepInEx.Bootstrap;
using HarmonyDebugPanel.Interfaces;

namespace HarmonyDebugPanel.Implementations.Collectors
{
    [ExcludeFromCodeCoverage(Justification = "Requires live BepInEx Chainloader runtime")]
    public sealed class ChainloaderPluginInfoSource : IPluginInfoSource
    {
        public IEnumerable<BepInExPluginInspectionInfo> GetPlugins()
        {
            List<BepInExPluginInspectionInfo> results = [];

            foreach (var pluginInfoEntry in Chainloader.PluginInfos)
            {
                var pluginInfo = pluginInfoEntry.Value;
                if (pluginInfo == null)
                {
                    continue;
                }

                var metadata = pluginInfo.Metadata;
                if (metadata == null || pluginInfo.Instance == null)
                {
                    continue;
                }

                var pluginAssembly = pluginInfo.Instance.GetType().Assembly;
                var assemblyName = pluginAssembly.GetName();
                var assemblyVersion = assemblyName.Version != null
                    ? assemblyName.Version.ToString()
                    : "Unknown";
                string location;

                try
                {
                    location = pluginAssembly.Location;
                }
                catch (NotSupportedException)
                {
                    location = string.Empty;
                }

                AssemblyInspectionInfo assemblyInfo = new(
                    assemblyName.Name ?? "Unknown",
                    assemblyVersion,
                    location,
                    pluginAssembly.GetReferencedAssemblies());

                results.Add(new BepInExPluginInspectionInfo(
                    metadata.GUID ?? string.Empty,
                    metadata.Name ?? metadata.GUID ?? "Unknown",
                    metadata.Version != null ? metadata.Version.ToString() : "Unknown",
                    assemblyInfo));
            }

            return results;
        }
    }
}
