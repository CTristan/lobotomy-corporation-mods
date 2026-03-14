// SPDX-License-Identifier: MIT

#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using LobotomyCorporationMods.Common.Attributes;
using LobotomyCorporationMods.Common.Constants;
using LobotomyCorporationMods.DebugPanel.Interfaces;

#endregion

namespace LobotomyCorporationMods.DebugPanel.Implementations
{
    /// <summary>
    ///     Reflects into BepInEx.Bootstrap.Chainloader.PluginInfos to extract plugin metadata.
    ///     Returns empty list if BepInEx is not loaded.
    /// </summary>
    [AdapterClass]
    [ExcludeFromCodeCoverage(Justification = Messages.UnityCodeCoverageJustification)]
    public sealed class ReflectionBepInExPluginInfoSource : IPluginInfoSource
    {
        private const BindingFlags PublicStatic = BindingFlags.Public | BindingFlags.Static;
        private const BindingFlags PublicInstance = BindingFlags.Public | BindingFlags.Instance;

        public IEnumerable<BepInExPluginInspectionInfo> GetPlugins()
        {
            var results = new List<BepInExPluginInspectionInfo>();

            try
            {
                Type chainloaderType = null;

                var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                foreach (var assembly in assemblies)
                {
                    if (assembly == null)
                    {
                        continue;
                    }

                    chainloaderType = assembly.GetType("BepInEx.Bootstrap.Chainloader");
                    if (chainloaderType != null)
                    {
                        break;
                    }
                }

                if (chainloaderType == null)
                {
                    return results;
                }

                var pluginInfosProp = chainloaderType.GetProperty("PluginInfos", PublicStatic);
                if (pluginInfosProp == null)
                {
                    return results;
                }

                if (!(pluginInfosProp.GetValue(null, null) is IDictionary pluginInfos))
                {
                    return results;
                }

                foreach (DictionaryEntry entry in pluginInfos)
                {
                    var pluginInfo = entry.Value;
                    if (pluginInfo == null)
                    {
                        continue;
                    }

                    try
                    {
                        var pluginInfoType = pluginInfo.GetType();

                        var metadataProp = pluginInfoType.GetProperty("Metadata", PublicInstance);
                        var instanceProp = pluginInfoType.GetProperty("Instance", PublicInstance);

                        if (metadataProp == null || instanceProp == null)
                        {
                            continue;
                        }

                        var metadata = metadataProp.GetValue(pluginInfo, null);
                        var instance = instanceProp.GetValue(pluginInfo, null);

                        if (metadata == null || instance == null)
                        {
                            continue;
                        }

                        var metadataType = metadata.GetType();
                        var guidProp = metadataType.GetProperty("GUID", PublicInstance);
                        var nameProp = metadataType.GetProperty("Name", PublicInstance);
                        var versionProp = metadataType.GetProperty("Version", PublicInstance);

                        var guid = guidProp != null ? guidProp.GetValue(metadata, null) as string ?? string.Empty : string.Empty;
                        var name = nameProp != null ? nameProp.GetValue(metadata, null) as string ?? "Unknown" : "Unknown";
                        var versionObj = versionProp?.GetValue(metadata, null);
                        var version = versionObj?.ToString() ?? "Unknown";

                        var pluginAssembly = instance.GetType().Assembly;
                        var assemblyName = pluginAssembly.GetName();
                        var assemblyVersion = assemblyName.Version != null ? assemblyName.Version.ToString() : "Unknown";
                        string assemblyLocation;

                        try
                        {
                            assemblyLocation = pluginAssembly.Location;
                        }
                        catch (NotSupportedException)
                        {
                            assemblyLocation = string.Empty;
                        }

                        var assemblyInfo = new AssemblyInspectionInfo(
                            assemblyName.Name ?? "Unknown",
                            assemblyVersion,
                            assemblyLocation,
                            pluginAssembly.GetReferencedAssemblies());

                        results.Add(new BepInExPluginInspectionInfo(guid, name, version, assemblyInfo));
                    }
                    catch (Exception)
                    {
                        // Skip individual plugin inspection failures
                    }
                }
            }
            catch (Exception)
            {
                // Return empty list if BepInEx is not available or reflection fails
            }

            return results;
        }
    }
}
