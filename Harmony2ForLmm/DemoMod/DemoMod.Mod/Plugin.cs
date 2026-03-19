// SPDX-License-Identifier: MIT

#region Using directives

using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using DemoMod.Mod.Patches;
using HarmonyLib;
using UnityEngine;

#endregion

namespace DemoMod.Mod
{
    /// <summary>
    /// Demonstrates: BepInEx plugin entry point (§BepInEx plugin entry point),
    /// built-in configuration system (§Configuration),
    /// config validation (§Config Validation),
    /// manual patch orchestration (§Manual Patching),
    /// and MonoBehaviour lifecycle (§MonoBehaviour Lifecycle).
    /// </summary>
    [BepInPlugin("com.example.demomod", "Demo Mod", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        internal static ManualLogSource Log { get; private set; }

        private static ConfigEntry<float> s_maxSuccessRate;
        private static ConfigEntry<float> s_energyMultiplier;

        private ConfigEntry<int> _damageMultiplier;
        private ConfigEntry<bool> _enableEnergyMultiplier;
        private bool _showOverlay;

        private void Awake()
        {
            Log = Logger;

            // §Configuration — Config.Bind creates a typed config entry backed by a .cfg file
            // §Config Validation — AcceptableValueRange constrains values and generates slider UI
            _damageMultiplier = Config.Bind(
                "General",
                "DamageMultiplier",
                1,
                new ConfigDescription(
                    "Multiplier applied to all damage",
                    new AcceptableValueRange<int>(1, 10)));

            s_maxSuccessRate = Config.Bind(
                "Difficulty",
                "MaxSuccessRate",
                0.95f,
                new ConfigDescription(
                    "Maximum work success probability (0.0 to 1.0)",
                    new AcceptableValueRange<float>(0.0f, 1.0f)));

            // §Manual Patching — config entries for the runtime-toggled energy multiplier
            _enableEnergyMultiplier = Config.Bind(
                "Energy",
                "EnableEnergyMultiplier",
                false,
                "Toggle the energy multiplier patch at runtime");

            s_energyMultiplier = Config.Bind(
                "Energy",
                "EnergyMultiplier",
                1.5f,
                new ConfigDescription(
                    "Multiplier applied to energy gains",
                    new AcceptableValueRange<float>(0.5f, 5.0f)));

            Logger.LogInfo($"[DemoMod:Config] DamageMultiplier = {_damageMultiplier.Value}");
            Logger.LogInfo($"[DemoMod:Config] MaxSuccessRate = {s_maxSuccessRate.Value}");
            Logger.LogInfo($"[DemoMod:ConfigValidation] DamageMultiplier range: 1–10, " +
                $"MaxSuccessRate range: 0.0–1.0, EnergyMultiplier range: 0.5–5.0");

            // §Configuration — react to config changes at runtime
            _damageMultiplier.SettingChanged += (sender, args) =>
            {
                Logger.LogInfo($"[DemoMod:Config] DamageMultiplier changed to {_damageMultiplier.Value}");
            };

            // §Manual Patching — SettingChanged handler applies/removes the patch at runtime
            _enableEnergyMultiplier.SettingChanged += (sender, args) =>
            {
                if (_enableEnergyMultiplier.Value)
                {
                    EnergyMultiplierManualPatch.ApplyPatch();
                }
                else
                {
                    EnergyMultiplierManualPatch.RemovePatch();
                }
            };

            // Apply the manual patch on startup if the config starts enabled
            if (_enableEnergyMultiplier.Value)
            {
                EnergyMultiplierManualPatch.ApplyPatch();
            }

            // §BepInEx plugin entry point — Harmony 2 patching API
            var harmony = new Harmony("com.example.demomod");
            harmony.PatchAll();
            Logger.LogInfo("[DemoMod:EntryPoint] All patches applied successfully");
        }

        /// <summary>
        /// §MonoBehaviour Lifecycle — Update() runs every frame.
        /// F9 toggles a debug overlay showing current config and patch status.
        /// Keep Update() lightweight to avoid FPS impact.
        /// </summary>
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F9))
            {
                _showOverlay = !_showOverlay;
                Logger.LogInfo($"[DemoMod:Lifecycle] Debug overlay toggled {(_showOverlay ? "ON" : "OFF")}");
            }
        }

        /// <summary>
        /// §MonoBehaviour Lifecycle — OnGUI() renders an IMGUI debug overlay.
        /// Called multiple times per frame (once per event) — keep rendering trivial.
        /// </summary>
        private void OnGUI()
        {
            if (!_showOverlay)
            {
                return;
            }

            var boxRect = new Rect(10, 10, 320, 140);
            GUI.Box(boxRect, "DemoMod Debug Overlay");
            GUI.Label(new Rect(20, 30, 300, 20), $"DamageMultiplier: {_damageMultiplier.Value}");
            GUI.Label(new Rect(20, 50, 300, 20), $"MaxSuccessRate: {s_maxSuccessRate.Value:F2}");
            GUI.Label(new Rect(20, 70, 300, 20), $"EnergyMultiplier: {s_energyMultiplier.Value:F2}");
            GUI.Label(new Rect(20, 90, 300, 20),
                $"Energy patch: {(EnergyMultiplierManualPatch.IsApplied ? "ACTIVE" : "inactive")}");
            GUI.Label(new Rect(20, 110, 300, 20), "Press F9 to close");
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

        /// <summary>
        /// Called by the manual patch prefix to get the current energy multiplier.
        /// §Manual Patching — Config + Manual Patch pipeline.
        /// </summary>
        public static float GetEnergyMultiplier()
        {
            return s_energyMultiplier.Value;
        }
    }
}
