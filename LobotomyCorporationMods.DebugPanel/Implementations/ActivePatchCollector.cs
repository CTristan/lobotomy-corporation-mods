// SPDX-License-Identifier: MIT

#region

using System.Collections.Generic;
using LobotomyCorporationMods.Common.Implementations;
using LobotomyCorporationMods.DebugPanel.Interfaces;
using LobotomyCorporationMods.Common.Models.Diagnostics;

#endregion

namespace LobotomyCorporationMods.DebugPanel.Implementations
{
    public sealed class ActivePatchCollector : IActivePatchCollector
    {
        private readonly IPatchInspectionSource _patchInspectionSource;

        public ActivePatchCollector(IPatchInspectionSource patchInspectionSource)
        {
            ThrowHelper.ThrowIfNull(patchInspectionSource);
            _patchInspectionSource = patchInspectionSource;
        }

        public IList<PatchInfo> Collect()
        {
            var results = new List<PatchInfo>();
            foreach (var patch in _patchInspectionSource.GetPatches())
            {
                if (patch == null)
                {
                    continue;
                }

                results.Add(new PatchInfo(
                    patch.TargetType,
                    patch.TargetMethod,
                    patch.PatchType,
                    patch.Owner,
                    patch.PatchMethod,
                    patch.PatchAssemblyName));
            }

            return results;
        }
    }
}
