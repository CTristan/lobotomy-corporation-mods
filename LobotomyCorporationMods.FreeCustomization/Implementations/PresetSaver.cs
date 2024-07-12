// SPDX-License-Identifier: MIT

using Customizing;
using LobotomyCorporationMods.Common.Interfaces;
using LobotomyCorporationMods.FreeCustomization.Constants;
using LobotomyCorporationMods.FreeCustomization.Extensions;
using LobotomyCorporationMods.FreeCustomization.Interfaces;
using LobotomyCorporationMods.FreeCustomization.Objects;
using UnityEngine;

namespace LobotomyCorporationMods.FreeCustomization.Implementations
{
    internal sealed class PresetSaver : IPresetSaver
    {
        private readonly IFileManager _fileManager;
        private readonly IPresetLoader _presetLoader;

        internal PresetSaver(IFileManager fileManager,
            IPresetLoader presetLoader)
        {
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
            var fileName = _fileManager.GetFile(PresetDefaults.DefaultCustomFileName);
            _fileManager.WriteAllText(fileName, jsonData);

            UpdateSavePresetButtonText(agentName, appearanceData);
        }

        public void UpdateSavePresetButtonText(string agentName,
            Appearance appearance)
        {
            if (Harmony_Patch.Instance.SavePresetButtonText == null)
            {
                AgentInfoWindow.currentWindow.CreateSavePresetButtonText();
            }

            _presetLoader.InitializeDefaultCustomPresetFile();
            Harmony_Patch.Instance.SavePresetButtonText.color = Harmony_Patch.Instance.PresetLoader.IsExactPreset(agentName, appearance) ? Color.grey : new Color(239f / 256f, 139f / 256f, 39f / 256f);
        }
    }
}
