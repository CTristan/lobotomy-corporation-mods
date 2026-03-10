// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using HarmonyDebugPanel.Interfaces;
using HarmonyDebugPanel.Models;

namespace HarmonyDebugPanel.Implementations.Collectors
{
    public sealed class AssemblyInfoCollector(IAssemblyInspectionSource assemblySource) : IInfoCollector<IList<AssemblyInfo>>
    {
        private readonly IAssemblyInspectionSource _assemblySource = assemblySource ?? throw new ArgumentNullException(nameof(assemblySource));

        public AssemblyInfoCollector()
            : this(new AppDomainAssemblyInspectionSource())
        {
        }

        public IList<AssemblyInfo> Collect()
        {
            var assemblies = new List<AssemblyInfo>();
            foreach (var assembly in _assemblySource.GetAssemblies())
            {
                if (assembly == null)
                {
                    continue;
                }

                var isHarmonyRelated = assembly.Name.IndexOf("harmony", StringComparison.OrdinalIgnoreCase) >= 0;
                assemblies.Add(new AssemblyInfo(
                    assembly.Name,
                    assembly.Version,
                    assembly.Location,
                    isHarmonyRelated));
            }

            return assemblies;
        }
    }
}
