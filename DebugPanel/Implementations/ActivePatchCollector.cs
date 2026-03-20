// SPDX-License-Identifier: MIT

#region

using System.Collections.Generic;
using DebugPanel.Common.Implementations;
using DebugPanel.Interfaces;
using DebugPanel.Common.Models.Diagnostics;

#endregion

namespace DebugPanel.Implementations
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
