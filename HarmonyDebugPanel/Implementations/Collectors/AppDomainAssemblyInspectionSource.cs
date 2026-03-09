// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyDebugPanel.Interfaces;

namespace HarmonyDebugPanel.Implementations.Collectors
{
    public sealed class AppDomainAssemblyInspectionSource : IAssemblyInspectionSource
    {
        public IEnumerable<AssemblyInspectionInfo> GetAssemblies()
        {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            List<AssemblyInspectionInfo> infos = new(assemblies.Length);

            foreach (Assembly assembly in assemblies)
            {
                if (assembly == null)
                {
                    continue;
                }

                AssemblyName assemblyName = assembly.GetName();
                string version = assemblyName.Version != null ? assemblyName.Version.ToString() : "Unknown";
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
