// SPDX-License-Identifier: MIT

#region Using directives

using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;

#endregion

namespace DemoMod.Mod.Patches
{
    /// <summary>
    /// Demonstrates: CodeMatcher pattern matching for transpilers (§CodeMatcher)
    /// combined with the Config + Transpiler pipeline (§Configuration).
    /// Replaces the hardcoded 0.95f success cap with a config-driven method call.
    /// </summary>
    [HarmonyPatch(typeof(UseSkill), "ProcessWorkTick")]
    public static class SuccessCapTranspiler
    {
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(
            IEnumerable<CodeInstruction> instructions)
        {
            Plugin.Log.LogInfo("[DemoMod:Transpiler] Patching ProcessWorkTick — replacing 0.95f cap");

            var getMaxRate = AccessTools.Method(
                typeof(Plugin), nameof(Plugin.GetMaxSuccessRate));

            // Guide: §CodeMatcher — fluent API to find and replace IL patterns
            var result = new CodeMatcher(instructions)
                .MatchForward(false,
                    new CodeMatch(OpCodes.Ldc_R4, 0.95f))
                .ThrowIfInvalid("Could not find 0.95f success cap")
                .SetInstructionAndAdvance(
                    new CodeInstruction(OpCodes.Call, getMaxRate))
                .MatchForward(false,
                    new CodeMatch(OpCodes.Ldc_R4, 0.95f))
                .ThrowIfInvalid("Could not find second 0.95f constant")
                .SetInstructionAndAdvance(
                    new CodeInstruction(OpCodes.Call, getMaxRate))
                .InstructionEnumeration();

            Plugin.Log.LogInfo("[DemoMod:Transpiler] ProcessWorkTick patched successfully");

            return result;
        }
    }
}
