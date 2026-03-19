// SPDX-License-Identifier: MIT

#region Using directives

using HarmonyLib;

#endregion

namespace DemoMod.Mod.Patches
{
    /// <summary>
    /// Demonstrates: Patch priority and ordering (§Patch Priority and Ordering).
    /// Two postfixes on CreatureManager.OnFixedUpdate with explicit priority and
    /// ordering attributes to guarantee execution order across mods.
    /// </summary>
    // Guide: §Patch Priority & Ordering — [HarmonyPriority] + [HarmonyBefore]/[HarmonyAfter]
    [HarmonyPatch(typeof(CreatureManager), "OnFixedUpdate")]
    [HarmonyPriority(Priority.High)]
    [HarmonyBefore("com.example.statuslogger")]
    public static class CreatureBuffPostfix
    {
        private static bool s_loggedOnce;

        /// <summary>
        /// Simulates applying a buff to creatures each tick.
        /// Runs first (High priority, before status logger) so buffs are applied
        /// before the status logger reads creature state.
        /// </summary>
        [HarmonyPostfix]
        public static void Postfix()
        {
            if (s_loggedOnce)
            {
                return;
            }

            s_loggedOnce = true;
            Plugin.Log.LogInfo("[DemoMod:Priority] CreatureBuffPostfix is active (Priority.High, runs first)");
        }
    }

    /// <summary>
    /// Demonstrates: Reading post-buff state with guaranteed ordering.
    /// Runs after CreatureBuffPostfix due to lower priority and [HarmonyAfter].
    /// </summary>
    [HarmonyPatch(typeof(CreatureManager), "OnFixedUpdate")]
    [HarmonyPriority(Priority.Low)]
    [HarmonyAfter("com.example.creaturebuff")]
    public static class CreatureStatusLoggerPostfix
    {
        private static bool s_loggedOnce;

        /// <summary>
        /// Reads creature state after buffs have been applied.
        /// Runs second (Low priority, after buff postfix) to ensure it sees updated state.
        /// </summary>
        [HarmonyPostfix]
        public static void Postfix()
        {
            if (s_loggedOnce)
            {
                return;
            }

            s_loggedOnce = true;
            Plugin.Log.LogInfo("[DemoMod:Priority] CreatureStatusLoggerPostfix is active (Priority.Low, runs second)");
        }
    }
}
