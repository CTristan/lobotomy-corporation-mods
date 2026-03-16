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
    public sealed class BaseModCollector : IInfoCollector<IList<DetectedModInfo>>
    {
        private static readonly HashSet<string> s_frameworkAssemblies = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "mscorlib",
            "System",
            "System.Core",
            "Microsoft.Xna.Framework",
            "Microsoft.Xna.Framework.Game",
            "UnityEditor",
            "UnityEngine",
            "UnityEngine.CoreModule",
            "UnityEngine.UI",
            "UnityEngine.Networking",
            "0Harmony",
            "0Harmony12",
            "0Harmony109",
            "BepInEx",
            "BepInEx.Core",
            "LobotomyBaseModLib",
            "BepInEx.ConfigurationManager",
            "Assembly-CSharp",
        };

        private readonly IPatchInspectionSource _patchInspectionSource;
        private readonly IHarmonyVersionClassifier _harmonyVersionClassifier;

        public BaseModCollector(IPatchInspectionSource patchInspectionSource, IHarmonyVersionClassifier harmonyVersionClassifier)
        {
            _patchInspectionSource = Guard.Against.Null(patchInspectionSource, nameof(patchInspectionSource));
            _harmonyVersionClassifier = Guard.Against.Null(harmonyVersionClassifier, nameof(harmonyVersionClassifier));
        }

        public IList<DetectedModInfo> Collect()
        {
            var patches = _patchInspectionSource.GetPatches();
            var patchesByAssembly = GroupPatchesByAssembly(patches);
            var mods = new List<DetectedModInfo>();

            foreach (var kvp in patchesByAssembly)
            {
                var assemblyName = kvp.Key;
                var assemblyPatches = kvp.Value;

                if (ShouldSkipAssembly(assemblyName))
                {
                    continue;
                }

                var firstPatch = assemblyPatches[0];
                var harmonyVersion = _harmonyVersionClassifier.Classify(firstPatch.PatchAssemblyReferences);

                mods.Add(new DetectedModInfo(
                    assemblyName,
                    firstPatch.PatchAssemblyVersion,
                    ModSource.Lmm,
                    harmonyVersion,
                    assemblyName,
                    string.Empty,
                    true,
                    assemblyPatches.Count,
                    0));
            }

            return mods;
        }

        private static Dictionary<string, List<PatchInspectionInfo>> GroupPatchesByAssembly(IEnumerable<PatchInspectionInfo> patches)
        {
            var grouped = new Dictionary<string, List<PatchInspectionInfo>>(StringComparer.OrdinalIgnoreCase);

            foreach (var patch in patches)
            {
                if (patch == null)
                {
                    continue;
                }

                if (string.IsNullOrEmpty(patch.PatchAssemblyName))
                {
                    continue;
                }

                if (!grouped.TryGetValue(patch.PatchAssemblyName, out var patchList))
                {
                    patchList = new List<PatchInspectionInfo>();
                    grouped[patch.PatchAssemblyName] = patchList;
                }

                patchList.Add(patch);
            }

            return grouped;
        }

        private static bool ShouldSkipAssembly(string assemblyName)
        {
            if (string.IsNullOrEmpty(assemblyName))
            {
                return true;
            }

            if (s_frameworkAssemblies.Contains(assemblyName))
            {
                return true;
            }

            return assemblyName.StartsWith("UnityEngine.", StringComparison.OrdinalIgnoreCase) ||
                   assemblyName.StartsWith("System.", StringComparison.OrdinalIgnoreCase) ||
                   assemblyName.StartsWith("Microsoft.", StringComparison.OrdinalIgnoreCase);
        }
    }
}
