// SPDX-License-Identifier: MIT

#region Using directives

using HarmonyLib;

#endregion

namespace DemoMod.Mod.Patches
{
    /// <summary>
    /// Demonstrates: Manual patching (§Manual Patching).
    /// Prefix on EnergyModel.AddEnergy that multiplies the added parameter by a config value.
    /// Uses a separate Harmony instance so it can be applied/removed independently at runtime.
    /// </summary>
    public static class EnergyMultiplierManualPatch
    {
        private static Harmony s_harmony;

        /// <summary>
        /// Whether the manual patch is currently applied.
        /// </summary>
        public static bool IsApplied { get; private set; }

        /// <summary>
        /// Applies the energy multiplier patch using manual Harmony API.
        /// Called from Plugin when the config toggle is enabled.
        /// </summary>
        public static void ApplyPatch()
        {
            if (IsApplied)
            {
                return;
            }

            if (s_harmony == null)
            {
                s_harmony = new Harmony("com.example.demomod.energytweak");
            }

            // Guide: §Manual Patching — harmony.Patch() applies a prefix/postfix at runtime
            var target = AccessTools.Method(typeof(EnergyModel), "AddEnergy", new[] { typeof(float) });
            var prefix = new HarmonyMethod(typeof(EnergyMultiplierManualPatch), nameof(Prefix));
            _ = s_harmony.Patch(target, prefix: prefix);
            IsApplied = true;

            Plugin.Log.LogInfo("[DemoMod:ManualPatch] Energy multiplier patch applied");
        }

        /// <summary>
        /// Removes the energy multiplier patch at runtime.
        /// Called from Plugin when the config toggle is disabled.
        /// </summary>
        public static void RemovePatch()
        {
            if (!IsApplied)
            {
                return;
            }

            // Guide: §Manual Patching — harmony.Unpatch() removes a specific patch at runtime
            var target = AccessTools.Method(typeof(EnergyModel), "AddEnergy", new[] { typeof(float) });
            s_harmony.Unpatch(target, HarmonyPatchType.Prefix, s_harmony.Id);
            IsApplied = false;

            Plugin.Log.LogInfo("[DemoMod:ManualPatch] Energy multiplier patch removed");
        }

        /// <summary>
        /// Prefix that multiplies the energy parameter before AddEnergy processes it.
        /// </summary>
        public static void Prefix(ref float added)
        {
            var multiplier = Plugin.GetEnergyMultiplier();
            added *= multiplier;
        }
    }
}
