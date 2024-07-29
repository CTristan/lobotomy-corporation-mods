// SPDX-License-Identifier: MIT

using UnityEngine;

namespace LobotomyCorporationMods.CustomizationOverhaul.Constants
{
    internal static class PresetConstants
    {
        internal const float AcceptDeletePresetButtonPositionX = 150f;
        internal const float AcceptDeletePresetButtonPositionY = 0f;
        internal const float DeletePresetButtonPositionX = 200f;
        internal const float DeletePresetButtonPositionY = 0f;
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

        internal const string JsonFileMask = "*.json";
        internal const string PresetsDirectoryName = "Presets";
        internal const string CustomFileName = "Presets/presets.json";
        internal const string PresetPanelImagePath = "Assets/preset-panel.png";
        internal const string PresetDeleteIconPath = "Assets/preset-delete-icon.png";
        internal const string PresetDeleteAcceptIconPath = "Assets/preset-delete-confirm-icon.png";
        internal const string PresetDeleteCancelIconPath = "Assets/preset-delete-cancel-icon.png";
        private const float PresetTextColorR = 0.93f;
        private const float PresetTextColorG = 0.54f;
        private const float PresetTextColorB = 0.15f;
        internal static Color PresetTextColor => new Color(PresetTextColorR, PresetTextColorG, PresetTextColorB);
    }
}
