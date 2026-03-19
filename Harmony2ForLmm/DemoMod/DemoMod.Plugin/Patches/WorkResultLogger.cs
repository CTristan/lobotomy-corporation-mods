// SPDX-License-Identifier: MIT

#region Using directives

using BepInEx;
using HarmonyLib;

#endregion

namespace DemoMod.Plugin.Patches
{
    /// <summary>
    /// Demonstrates: Dependency declarations (§Dependency declarations).
    /// Soft dependency on a hypothetical "Custom Creatures" mod — detects it at startup
    /// and branches patch behavior accordingly.
    /// </summary>
    [BepInPlugin("com.example.xpoverhaul", "XP Overhaul", "1.0.0")]
    [BepInDependency("com.example.customcreatures",
        BepInDependency.DependencyFlags.SoftDependency)]
    public class XpOverhaulPlugin : BaseUnityPlugin
    {
        internal static bool HasCustomCreatures { get; private set; }

        private void Awake()
        {
            // §Dependency declarations — check at startup whether the optional mod is loaded
            HasCustomCreatures = BepInEx.Bootstrap.Chainloader.PluginInfos
                .ContainsKey("com.example.customcreatures");

            if (HasCustomCreatures)
            {
                Logger.LogInfo("[DemoMod:Dependencies] Custom Creatures detected — " +
                    "adjusting XP formula for non-standard risk levels");
            }
            else
            {
                Logger.LogInfo("[DemoMod:Dependencies] Custom Creatures not present — " +
                    "using standard XP formula");
            }

            var harmony = new Harmony("com.example.xpoverhaul");
            harmony.PatchAll();
        }
    }

    /// <summary>
    /// Demonstrates: Branching patch behavior based on soft dependency detection.
    /// </summary>
    [HarmonyPatch(typeof(UseSkill), "FinishWorkSuccessfully")]
    public static class WorkResultLogger
    {
        private static bool s_loggedOnce;

        [HarmonyPostfix]
        public static void Postfix(UseSkill __instance)
        {
            if (!s_loggedOnce)
            {
                Plugin.Log.LogInfo("[DemoMod:Dependencies] WorkResultLogger postfix is active");
                s_loggedOnce = true;
            }

            if (XpOverhaulPlugin.HasCustomCreatures)
            {
                // Custom creatures use extended risk levels (6-10) that the base
                // XP formula doesn't handle — apply adjusted multipliers
                Plugin.Log.LogDebug("[DemoMod:Dependencies] Using custom creature XP path");
            }
            else
            {
                // Standard XP override for vanilla creatures (risk levels 1-5)
                Plugin.Log.LogDebug("[DemoMod:Dependencies] Using standard XP path");
            }
        }
    }
}
