// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using HarmonyDebugPanel.Interfaces;
using HarmonyDebugPanel.Models;

namespace HarmonyDebugPanel.Implementations.Collectors
{
    public sealed class ActivePatchCollector(IPatchInspectionSource patchInspectionSource) : IInfoCollector<IList<PatchInfo>>
    {
        private readonly IPatchInspectionSource _patchInspectionSource = patchInspectionSource ?? throw new ArgumentNullException(nameof(patchInspectionSource));

        public ActivePatchCollector()
            : this(new HarmonyPatchInspectionSource())
        {
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
