// SPDX-License-Identifier: MIT

using JetBrains.Annotations;
using UnityEngine;

namespace LobotomyCorporationMods.CustomizationOverhaul.Constants
{
    internal static class PresetConstants
    {
        internal const float LoadPresetButtonPositionX = 570f;
        internal const float LoadPresetButtonPositionY = -300f;
        internal const float LoadPresetPanelPositionX = 340f;
        internal const float LoadPresetPanelPositionY = 180f;
        internal const float LoadPresetPanelSizeX = 520f;
        internal const float LoadPresetPanelSizeY = 600f;
        internal const float SavePresetButtonPositionX = 570f;
        internal const float SavePresetButtonPositionY = -490f;
        internal const float ButtonSizeX = 120f;
        internal const float ButtonSizeY = 120f;
        internal const int ButtonTextFontSize = 30;

        private const float PresetTextColorR = 0.93f;
        private const float PresetTextColorG = 0.54f;
        private const float PresetTextColorB = 0.15f;

        [NotNull]
        internal static string CustomFileName => "Presets/presets.json";

        internal static Color PresetTextColor => new Color(PresetTextColorR, PresetTextColorG, PresetTextColorB);
    }
}
