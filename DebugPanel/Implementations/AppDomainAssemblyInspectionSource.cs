// SPDX-License-Identifier: MIT

#region

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using DebugPanel.Common.Attributes;
using DebugPanel.Common.Constants;
using DebugPanel.Interfaces;

#endregion

namespace DebugPanel.Implementations
{
    [AdapterClass]
    [ExcludeFromCodeCoverage(Justification = Messages.UnityCodeCoverageJustification)]
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
