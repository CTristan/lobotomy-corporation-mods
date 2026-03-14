// SPDX-License-Identifier: MIT

#region

using System;
using System.Collections.Generic;
using LobotomyCorporationMods.Common.Extensions;
using LobotomyCorporationMods.Common.Implementations;
using LobotomyCorporationMods.DebugPanel.Interfaces;
using LobotomyCorporationMods.DebugPanel.Models;

#endregion

namespace LobotomyCorporationMods.DebugPanel.Implementations
{
    public sealed class AssemblyInfoCollector : IInfoCollector<IList<AssemblyInfo>>
    {
        private readonly IAssemblyInspectionSource _assemblySource;

        public AssemblyInfoCollector(IAssemblyInspectionSource assemblySource)
        {
            _assemblySource = Guard.Against.Null(assemblySource, nameof(assemblySource));
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
