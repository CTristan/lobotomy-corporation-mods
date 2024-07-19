// SPDX-License-Identifier: MIT

using JetBrains.Annotations;
using UnityEngine;

namespace LobotomyCorporationMods.CustomizationOverhaul.Constants
{
    internal static class PresetConstants
    {
        internal const float LoadPresetButtonPositionX = 595f;
        internal const float LoadPresetButtonPositionY = -400f;
        internal const float LoadPresetPanelPositionX = 500f;
        internal const float LoadPresetPanelPositionY = 30f;
        internal const float SavePresetButtonPositionX = 595f;
        internal const float SavePresetButtonPositionY = -490f;
        internal const float ButtonSizeX = 150f;
        internal const float ButtonSizeY = 90f;
        internal const int ButtonTextFontSize = 30;

        private const float PresetTextColorR = 0.93f;
        private const float PresetTextColorG = 0.54f;
        private const float PresetTextColorB = 0.15f;

        [NotNull]
        internal static string CustomFileName => "Presets/presets.json";

        internal static Color PresetTextColor => new Color(PresetTextColorR, PresetTextColorG, PresetTextColorB);
    }
}
