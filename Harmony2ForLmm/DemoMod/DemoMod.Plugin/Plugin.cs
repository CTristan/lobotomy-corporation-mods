// SPDX-License-Identifier: MIT

#region Using directives

using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;

#endregion

namespace DemoMod.Plugin
{
    /// <summary>
    /// Demonstrates: BepInEx plugin entry point (§BepInEx plugin entry point)
    /// and built-in configuration system (§Configuration).
    /// </summary>
    [BepInPlugin("com.example.demomod", "Demo Mod", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        internal static ManualLogSource Log { get; private set; }

        private static ConfigEntry<float> s_maxSuccessRate;

        private ConfigEntry<int> _damageMultiplier;

        private void Awake()
        {
            Log = Logger;

            // §Configuration — Config.Bind creates a typed config entry backed by a .cfg file
            _damageMultiplier = Config.Bind(
                "General",
                "DamageMultiplier",
                1,
                "Multiplier applied to all damage");

            s_maxSuccessRate = Config.Bind(
                "Difficulty",
                "MaxSuccessRate",
                0.95f,
                "Maximum work success probability (0.0 to 1.0)");

            Logger.LogInfo($"[DemoMod:Config] DamageMultiplier = {_damageMultiplier.Value}");
            Logger.LogInfo($"[DemoMod:Config] MaxSuccessRate = {s_maxSuccessRate.Value}");

            // §Configuration — react to config changes at runtime
            _damageMultiplier.SettingChanged += (sender, args) =>
            {
                Logger.LogInfo($"[DemoMod:Config] DamageMultiplier changed to {_damageMultiplier.Value}");
            };

            // §BepInEx plugin entry point — Harmony 2 patching API
            var harmony = new Harmony("com.example.demomod");
            harmony.PatchAll();
            Logger.LogInfo("[DemoMod:EntryPoint] All patches applied successfully");
        }

        /// <summary>
        /// Called by the Transpiler at runtime instead of the hardcoded 0.95f.
        /// §Configuration — Config + Transpiler pipeline.
        /// </summary>
        public static float GetMaxSuccessRate()
        {
            var value = s_maxSuccessRate.Value;
            Log.LogDebug($"[DemoMod:Transpiler] GetMaxSuccessRate() called, returning {value}");

            return value;
        }
    }
}
