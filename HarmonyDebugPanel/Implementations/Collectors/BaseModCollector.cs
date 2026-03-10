// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using HarmonyDebugPanel.Interfaces;
using HarmonyDebugPanel.Models;

namespace HarmonyDebugPanel.Implementations.Collectors
{
    public sealed class BaseModCollector(
        IPatchInspectionSource patchInspectionSource,
        IHarmonyVersionClassifier harmonyVersionClassifier) : IInfoCollector<IList<ModInfo>>
    {
        private static readonly HashSet<string> s_frameworkAssemblies = new(StringComparer.OrdinalIgnoreCase)
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

        private readonly IPatchInspectionSource _patchInspectionSource = patchInspectionSource ?? throw new ArgumentNullException(nameof(patchInspectionSource));
        private readonly IHarmonyVersionClassifier _harmonyVersionClassifier = harmonyVersionClassifier ?? throw new ArgumentNullException(nameof(harmonyVersionClassifier));

        public BaseModCollector()
            : this(new HarmonyPatchInspectionSource(), new HarmonyVersionClassifier())
        {
        }

        public IList<ModInfo> Collect()
        {
            var patches = _patchInspectionSource.GetPatches();
            var patchesByAssembly = GroupPatchesByAssembly(patches);
            var mods = new List<ModInfo>();

            foreach (var kvp in patchesByAssembly)
            {
                var assemblyName = kvp.Key;
                var assemblyPatches = kvp.Value;

                if (ShouldSkipAssembly(assemblyName))
                {
                    continue;
                }

                // Use the first patch's assembly info for version and references
                var firstPatch = assemblyPatches[0];
                var harmonyVersion = _harmonyVersionClassifier.Classify(firstPatch.PatchAssemblyReferences);

                // Only include Harmony 1 mods
                if (harmonyVersion != HarmonyVersion.Harmony1)
                {
                    continue;
                }

                mods.Add(new ModInfo(
                    assemblyName,
                    firstPatch.PatchAssemblyVersion,
                    ModSource.Lmm,
                    harmonyVersion,
                    assemblyName,
                    string.Empty,
                    true,
                    assemblyPatches.Count));
            }

            return mods;
        }

        private static Dictionary<string, List<PatchInspectionInfo>> GroupPatchesByAssembly(IEnumerable<PatchInspectionInfo> patches)
        {
            var grouped = new Dictionary<string, List<PatchInspectionInfo>>(StringComparer.OrdinalIgnoreCase);

            foreach (var patch in patches)
            {
                // Skip null patches (defensive)
                if (patch == null)
                {
                    continue;
                }

                // Skip patches with empty assembly names
                // Note: null is not allowed by PatchInspectionInfo constructor (throws ArgumentNullException)
                if (string.IsNullOrEmpty(patch.PatchAssemblyName))
                {
                    continue;
                }

                if (!grouped.TryGetValue(patch.PatchAssemblyName, out var patchList))
                {
                    patchList = [];
                    grouped[patch.PatchAssemblyName] = patchList;
                }

                patchList.Add(patch);
            }

            return grouped;
        }

        private static bool ShouldSkipAssembly(string assemblyName)
        {
            // Empty string indicates "no assembly name" - skip these
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
