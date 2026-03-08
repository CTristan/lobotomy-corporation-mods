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

        public KeyCode OverlayToggleHotkey { get; private set; } = KeyCode.F9;

        public KeyCode RefreshHotkey { get; private set; } = KeyCode.F10;

        public bool ShowBepInExPlugins { get; private set; } = true;

        public bool ShowLmmMods { get; private set; } = true;

        public bool ShowActivePatches { get; private set; } = true;

        public bool ShowAssemblyInfo { get; private set; } = true;

        public static PluginConfiguration Bind(ConfigFile config)
        {
            var settings = new PluginConfiguration();
            if (config == null)
            {
                return settings;
            }

            settings.OverlayToggleHotkey = config.Bind(
                "General",
                "OverlayToggleHotkey",
                KeyCode.F9,
                new ConfigDescription("Hotkey used to toggle the Harmony diagnostic overlay.")).Value;
            settings.RefreshHotkey = config.Bind(
                "General",
                "RefreshHotkey",
                KeyCode.F10,
                new ConfigDescription("Hotkey used to refresh diagnostic data.")).Value;
            settings.ShowBepInExPlugins = config.Bind(
                "Sections",
                "ShowBepInExPlugins",
                true,
                new ConfigDescription("Show the BepInEx plugins section in the overlay.")).Value;
            settings.ShowLmmMods = config.Bind(
                "Sections",
                "ShowLmmMods",
                true,
                new ConfigDescription("Show the LMM/Basemod mods section in the overlay.")).Value;
            settings.ShowActivePatches = config.Bind(
                "Sections",
                "ShowActivePatches",
                true,
                new ConfigDescription("Show the active Harmony patches section in the overlay.")).Value;
            settings.ShowAssemblyInfo = config.Bind(
                "Sections",
                "ShowAssemblyInfo",
                true,
                new ConfigDescription("Show the loaded assembly info section in the overlay.")).Value;

            return settings;
        }
    }
}
