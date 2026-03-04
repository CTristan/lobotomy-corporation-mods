using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Mono.Cecil;

namespace RetargetHarmony
{
    public static class RetargetHarmony
    {
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
            var refs = asm.MainModule.AssemblyReferences;
            var changed = false;

            // 1) Find all Harmony references (both old and potentially already retargeted)
            var harmonyRefs = refs.Where(r => r.Name == "0Harmony" || r.Name == "0Harmony109").ToList();

            if (harmonyRefs.Count > 0)
            {
                // Ensure the first reference points to 0Harmony109
                if (harmonyRefs[0].Name != "0Harmony109")
                {
                    harmonyRefs[0].Name = "0Harmony109";
                    changed = true;
                }

                // 2) Defensive sweep: remove any duplicate Harmony metadata references
                for (int i = 1; i < harmonyRefs.Count; i++)
                {
                    refs.Remove(harmonyRefs[i]);
                    changed = true;
                }
            }

            Trace.WriteLine(changed ? $"[RetargetHarmony_v5] Rewrote reference 0Harmony -> 0Harmony109 in {asm.Name.Name}" : $"[RetargetHarmony_v5] No 0Harmony reference found in {asm.Name.Name}; nothing changed.");
        }
    }
}
