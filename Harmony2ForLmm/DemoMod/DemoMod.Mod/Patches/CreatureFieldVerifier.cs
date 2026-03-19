// SPDX-License-Identifier: MIT

#region Using directives

using System;
using System.Reflection;
using HarmonyLib;

#endregion

namespace DemoMod.Mod.Patches
{
    /// <summary>
    /// Verifies: Preloader patcher (§Preloader patchers).
    /// Postfix on CreatureModel.OnGameInit that checks whether the
    /// customDifficultyLevel field was injected by the preloader patcher.
    /// </summary>
    [HarmonyPatch(typeof(CreatureModel), "OnGameInit")]
    public static class CreatureFieldVerifier
    {
        private static bool s_loggedOnce;

        [HarmonyPostfix]
        public static void Postfix(CreatureModel __instance)
        {
            if (s_loggedOnce)
            {
                return;
            }

            s_loggedOnce = true;

            try
            {
                // Guide: §Preloader patchers — verify the field injected by DemoMod.Patcher
                var field = typeof(CreatureModel).GetField(
                    "customDifficultyLevel",
                    BindingFlags.Public | BindingFlags.Instance);

                if (field != null)
                {
                    var value = field.GetValue(__instance);
                    Plugin.Log.LogInfo(
                        $"[DemoMod:Preloader] customDifficultyLevel field found on CreatureModel, value = {value}");

                    // Write and read back to confirm it's a real field
                    field.SetValue(__instance, 42);
                    var readBack = (int)field.GetValue(__instance);
                    Plugin.Log.LogInfo(
                        $"[DemoMod:Preloader] Write/read test: wrote 42, read back {readBack} — " +
                        (readBack == 42 ? "PASS" : "FAIL"));
                }
                else
                {
                    Plugin.Log.LogWarning(
                        "[DemoMod:Preloader] customDifficultyLevel field NOT found — " +
                        "preloader patcher may not be installed in BepInEx/patchers/");
                }
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"[DemoMod:Preloader] Field verification failed: {ex}");
            }
        }
    }
}
