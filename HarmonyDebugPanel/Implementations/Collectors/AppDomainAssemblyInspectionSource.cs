// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using HarmonyDebugPanel.Interfaces;

namespace HarmonyDebugPanel.Implementations.Collectors
{
    public sealed class AppDomainAssemblyInspectionSource : IAssemblyInspectionSource
    {
        public IEnumerable<AssemblyInspectionInfo> GetAssemblies()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var infos = new List<AssemblyInspectionInfo>(assemblies.Length);

            foreach (var assembly in assemblies)
            {
                if (assembly == null)
                {
                    continue;
                }

                var assemblyName = assembly.GetName();
                var version = assemblyName.Version != null ? assemblyName.Version.ToString() : "Unknown";
                string location;

                try
                {
                    location = assembly.Location;
                }
                catch (NotSupportedException)
                {
                    location = string.Empty;
                }

                infos.Add(new AssemblyInspectionInfo(
                    assemblyName.Name ?? "Unknown",
                    version,
                    location,
                    assembly.GetReferencedAssemblies()));
            }

            return infos;
        }
    }
}
