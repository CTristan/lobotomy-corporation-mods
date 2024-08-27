// SPDX-License-Identifier: MIT

using UnityEngine;

namespace LobotomyCorporationMods.ProjectNugway.Constants
{
    internal static class UiComponentConstants
    {
        internal const string AcceptDeletePresetIconPath = "Assets/preset-delete-confirm-icon.png";
        internal const float AcceptDeletePresetButtonPositionX = 200f;
        internal const float AcceptDeletePresetButtonPositionY = 0f;

        internal const string CancelDeletePresetIconPath = "Assets/preset-delete-cancel-icon.png";
        internal const float CancelDeletePresetButtonPositionX = -200f;
        internal const float CancelDeletePresetButtonPositionY = 0f;

        internal const string DeletePresetIconPath = "Assets/preset-delete-icon.png";
        internal const float DeletePresetButtonPositionX = -200f;
        internal const float DeletePresetButtonPositionY = 0f;

        internal const string DeletePresetPanelImagePath = "Assets/delete-preset-panel.png";

        // internal const string PresetButtonBorderTopImagePath = "Assets/preset-button-border-top.png";
        // internal const string PresetButtonBorderBottomImagePath = "Assets/preset-button-border-bottom.png";
        internal const string PresetButtonBorderImagePath = "Assets/preset-button-border.png";
        internal const float PresetButtonBorderPositionX = 0f;
        internal const float PresetButtonBorderPositionY = -53.51f;
        internal const float ButtonSizeX = 120f;
        internal const float ButtonSizeY = 104f;
        internal const int ButtonTextFontSize = 30;
        internal const float PresetButtonBorderScale = 1.8096f;
        internal const float PresetButtonBorderSizeX = 67.6f;
        internal const float PresetButtonBorderSizeY = 8.9f;
        internal const float LoadPresetButtonPositionY = -171.8f;
        internal const float LoadPresetPanelPositionX = 340f;
        internal const float LoadPresetPanelPositionY = 180f;
        internal const float LoadPresetPanelSizeX = 520f;
        internal const float LoadPresetPanelSizeY = 600f;
        internal const float SavePresetButtonPositionY = -282.1f;

        internal const string JsonFileMask = "*.json";
        internal const string PresetsDirectoryName = "Presets";
        internal const string CustomFileName = "Presets/presets.json";
        internal const string PresetPanelImagePath = "Assets/preset-panel.png";
        private const float PresetTextColorR = 1f;
        private const float PresetTextColorG = 0.580f;
        private const float PresetTextColorB = 0.254f;
        internal static Color PresetTextColor => new Color(PresetTextColorR, PresetTextColorG, PresetTextColorB);
    }
}
