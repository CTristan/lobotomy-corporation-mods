// SPDX-License-Identifier: MIT

using System;
using BepInEx.Configuration;
using UnityEngine;

namespace HarmonyDebugPanel
{
    public sealed class PluginConfiguration
    {
        private PluginConfiguration()
        {
        }

        public ConfigEntry<KeyCode> OverlayToggleHotkey { get; private set; }

        public ConfigEntry<KeyCode> RefreshHotkey { get; private set; }

        public ConfigEntry<bool> ShowBepInExPlugins { get; private set; }

        public ConfigEntry<bool> ShowLmmMods { get; private set; }

        public ConfigEntry<bool> ShowActivePatches { get; private set; }

        public ConfigEntry<bool> ShowAssemblyInfo { get; private set; }

        public static PluginConfiguration Bind(ConfigFile config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            var settings = new PluginConfiguration
            {
                OverlayToggleHotkey = config.Bind(
                    "General",
                    "OverlayToggleHotkey",
                    KeyCode.F9,
                    new ConfigDescription("Hotkey used to toggle the Harmony diagnostic overlay.")),
                RefreshHotkey = config.Bind(
                    "General",
                    "RefreshHotkey",
                    KeyCode.F10,
                    new ConfigDescription("Hotkey used to refresh diagnostic data.")),
                ShowBepInExPlugins = config.Bind(
                    "Sections",
                    "ShowBepInExPlugins",
                    true,
                    new ConfigDescription("Show the BepInEx plugins section in the overlay.")),
                ShowLmmMods = config.Bind(
                    "Sections",
                    "ShowLmmMods",
                    true,
                    new ConfigDescription("Show the LMM/Basemod mods section in the overlay.")),
                ShowActivePatches = config.Bind(
                    "Sections",
                    "ShowActivePatches",
                    true,
                    new ConfigDescription("Show the active Harmony patches section in the overlay.")),
                ShowAssemblyInfo = config.Bind(
                    "Sections",
                    "ShowAssemblyInfo",
                    true,
                    new ConfigDescription("Show the loaded assembly info section in the overlay.")),
            };

            return settings;
        }
    }
}
