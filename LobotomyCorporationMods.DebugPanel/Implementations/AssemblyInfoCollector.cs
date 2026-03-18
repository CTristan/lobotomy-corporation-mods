// SPDX-License-Identifier: MIT

#region

using System;
using System.Collections.Generic;
using Hemocode.Common.Implementations;
using Hemocode.DebugPanel.Interfaces;
using Hemocode.Common.Models.Diagnostics;

#endregion

namespace Hemocode.DebugPanel.Implementations
{
    public sealed class AssemblyInfoCollector : IInfoCollector<IList<AssemblyInfo>>
    {
        private readonly IAssemblyInspectionSource _assemblySource;

        public AssemblyInfoCollector(IAssemblyInspectionSource assemblySource)
        {
            ThrowHelper.ThrowIfNull(assemblySource);
            _assemblySource = assemblySource;
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
                    isHarmonyRelated,
                    assembly.References));
            }

            return assemblies;
        }
    }
}
