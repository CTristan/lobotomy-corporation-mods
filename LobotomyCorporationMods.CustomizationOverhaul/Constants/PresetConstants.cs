// SPDX-License-Identifier: MIT

using JetBrains.Annotations;
using UnityEngine;

namespace LobotomyCorporationMods.CustomizationOverhaul.Constants
{
    internal static class PresetConstants
    {
        internal const float PresetButtonPositionX = 595f;
        internal const float PresetButtonPositionY = -490f;
        internal const int PresetButtonTextFontSize = 30;

        private const float PresetTextColorR = 0.93f;
        private const float PresetTextColorG = 0.54f;
        private const float PresetTextColorB = 0.15f;

        [NotNull]
        internal static string CustomFileName => "Presets/presets.json";

        internal static Color PresetTextColor => new Color(PresetTextColorR, PresetTextColorG, PresetTextColorB);
    }
}
