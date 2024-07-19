// SPDX-License-Identifier: MIT

using Customizing;
using LobotomyCorporationMods.Common.Extensions;
using LobotomyCorporationMods.Common.Interfaces;
using LobotomyCorporationMods.CustomizationOverhaul.Constants;
using LobotomyCorporationMods.CustomizationOverhaul.Extensions;
using LobotomyCorporationMods.CustomizationOverhaul.Interfaces;
using LobotomyCorporationMods.CustomizationOverhaul.Objects;
using UnityEngine;

namespace LobotomyCorporationMods.CustomizationOverhaul.Implementations
{
    internal sealed class PresetSaver : IPresetSaver
    {
        private readonly IFileManager _fileManager;
        private readonly IPresetLoader _presetLoader;

        internal PresetSaver(IFileManager fileManager,
            IPresetLoader presetLoader)
        {
            if (fileManager.IsNull())
            {
                return;
            }

            _fileManager = fileManager;
            _presetLoader = presetLoader;
        }

        public void SavePreset()
        {
            var customizingWindow = CustomizingWindow.CurrentWindow;
            var agentName = customizingWindow.appearanceUI.copied.CustomName;
            var appearanceData = customizingWindow.appearanceUI.copied.appearance;

            // Reload the default custom preset file in case there are any missing changes before we overwrite the file.
            var data = _presetLoader.LoadPresetsFromDefaultCustomFile();
            data.Presets[agentName] = PresetData.FromAppearanceData(appearanceData);

            var jsonData = data.ToJson();
            var fileName = _fileManager.GetFile(PresetConstants.CustomFileName);
            _fileManager.WriteAllText(fileName, jsonData);

            UpdateSavePresetButtonText(agentName, appearanceData);
        }

        public void UpdateSavePresetButtonText(string agentName,
            Appearance appearance)
        {
            if (Harmony_Patch.Instance.SavePresetButton.IsNull())
            {
                AgentInfoWindow.currentWindow.CreateSavePresetButton();
            }

            _presetLoader.InitializeDefaultCustomPresetFile();
            Harmony_Patch.Instance.SavePresetButton.TextColor = Harmony_Patch.Instance.PresetLoader.IsExactPreset(agentName, appearance) ? Color.grey : PresetConstants.PresetTextColor;
        }
    }
}
