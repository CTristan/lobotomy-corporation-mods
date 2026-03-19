// SPDX-License-Identifier: MIT

#region Using directives

using System;
using HarmonyLib;

#endregion

namespace DemoMod.Mod.Patches
{
    /// <summary>
    /// Demonstrates: Reverse patches (§Reverse Patch).
    /// Creates a callable copy of UseSkill.CalculateLevelExp — type-safe, no reflection.
    /// </summary>
    [HarmonyPatch(typeof(UseSkill))]
    public static class XpFormulaReversePatch
    {
        // Guide: §Reverse Patch — creates a callable copy of a private method
        [HarmonyReversePatch]
        [HarmonyPatch("CalculateLevelExp")]
        public static float CalculateLevelExp(UseSkill instance, RwbpType rwbpType)
        {
            // Guide: §Reverse Patch — stub body is replaced at runtime with original method's IL
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Postfix on UseSkill.FinishWorkSuccessfully that exercises the Reverse Patch.
    /// When work completes, logs the XP multipliers for all four RWBP stats using
    /// the reverse-patched CalculateLevelExp.
    /// </summary>
    [HarmonyPatch(typeof(UseSkill), "FinishWorkSuccessfully")]
    public static class XpFormulaLogger
    {
        private static bool s_loggedOnce;

        [HarmonyPostfix]
        public static void Postfix(UseSkill __instance)
        {
            if (!s_loggedOnce)
            {
                Plugin.Log.LogInfo("[DemoMod:ReversePatch] Exercising CalculateLevelExp reverse patch");
                s_loggedOnce = true;
            }

            try
            {
                var r = XpFormulaReversePatch.CalculateLevelExp(__instance, RwbpType.R);
                var w = XpFormulaReversePatch.CalculateLevelExp(__instance, RwbpType.W);
                var b = XpFormulaReversePatch.CalculateLevelExp(__instance, RwbpType.B);
                var p = XpFormulaReversePatch.CalculateLevelExp(__instance, RwbpType.P);
                Plugin.Log.LogInfo(
                    $"[DemoMod:ReversePatch] XP multipliers — R:{r:F2} W:{w:F2} B:{b:F2} P:{p:F2}");
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"[DemoMod:ReversePatch] Failed to call CalculateLevelExp: {ex}");
            }
        }
    }
}
