// SPDX-License-Identifier: MIT

#pragma warning disable CA1724 // Type name conflicts with namespace name (required by BepInEx contract)

using System.Collections.Generic;
using System.Linq;
using BepInEx.Logging;
using Mono.Cecil;

namespace RetargetHarmony
{
    public static class RetargetHarmony
    {
        // Static log source managed by BepInEx framework (long-lived)
        private static readonly ManualLogSource Log = Logger.CreateLogSource("RetargetHarmony");

        // BepInEx 5 requires static property returning names of assemblies to patch
        public static IEnumerable<string> TargetDLLs
        {
            get
            {
                yield return "Assembly-CSharp.dll";
                yield return "LobotomyBaseModLib.dll";
            }
        }

        // BepInEx 5 requires static Patch method
        public static void Patch(AssemblyDefinition asm)
        {
            if (asm == null)
            {
                throw new System.ArgumentNullException(nameof(asm));
            }

            var refs = asm.MainModule.AssemblyReferences;
            var changed = false;

            // 1) Find all Harmony references (both old and potentially already retargeted)
            var harmonyRefs = refs.Where(r => r.Name == "0Harmony" || r.Name == "0Harmony109").ToList();

            // Defensive check: ToList() should never return null, but guard against unexpected behavior
            if (harmonyRefs != null && harmonyRefs.Count > 0)
            {
                // Ensure the first reference points to 0Harmony109
                if (harmonyRefs[0].Name != "0Harmony109")
                {
                    harmonyRefs[0].Name = "0Harmony109";
                    changed = true;
                }

                // 2) Defensive sweep: remove any duplicate Harmony metadata references
                // Safe to modify refs while iterating over harmonyRefs (a separate list copy)
                for (int i = 1; i < harmonyRefs.Count; i++)
                {
                    refs.Remove(harmonyRefs[i]);
                    changed = true;
                }
            }

            if (changed)
            {
                Log.LogInfo($"Rewrote reference 0Harmony -> 0Harmony109 in {asm.Name.Name}");
            }
            else
            {
                Log.LogInfo($"No 0Harmony reference found in {asm.Name.Name}; nothing changed.");
            }
        }
    }
}
