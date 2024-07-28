// SPDX-License-Identifier: MIT

using Customizing;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Extensions;
using LobotomyCorporationMods.Common.Interfaces;
using LobotomyCorporationMods.CustomizationOverhaul.Constants;
using LobotomyCorporationMods.CustomizationOverhaul.Extensions;
using LobotomyCorporationMods.CustomizationOverhaul.Interfaces;
using LobotomyCorporationMods.CustomizationOverhaul.Objects;
using UnityEngine;

namespace LobotomyCorporationMods.CustomizationOverhaul.Implementations
{
    internal sealed class PresetWriter : IPresetWriter
    {
        private readonly IFileManager _fileManager;
        private readonly IPresetLoader _presetLoader;

        internal PresetWriter([NotNull] IFileManager fileManager,
            IPresetLoader presetLoader)
        {
            if (fileManager.IsNull())
            {
                return;
            }

            _fileManager = fileManager;
            _presetLoader = presetLoader;
        }

        /// <summary>Deletes the specified preset from all preset files.</summary>
        /// <param name="presetName">The name of the preset to delete.</param>
        public void DeletePreset(string presetName)
        {
            var presetFiles = _presetLoader.FindAllPresetFiles();

            foreach (var presetFile in presetFiles)
            {
                var presets = _presetLoader.LoadPresetsFromCustomFile(presetFile);
                if (presets.Presets.Remove(presetName))
                {
                    SavePresetListToFile(presets, presetFile);
                }
            }
        }

        public void SavePreset()
        {
            var customizingWindow = CustomizingWindow.CurrentWindow;
            var agentName = customizingWindow.appearanceUI.copied.CustomName;
            var appearanceData = customizingWindow.appearanceUI.copied.appearance;

            // Reload the default custom preset file in case there are any missing changes before we overwrite the file.
            var data = _presetLoader.LoadPresetsFromCustomFile();
            data.Presets[agentName] = PresetData.FromAppearanceData(appearanceData);

            SavePresetListToFile(data, _fileManager.GetFile(PresetConstants.CustomFileName));

            UpdateSavePresetButtonText(agentName, appearanceData);
        }

        public void UpdateSavePresetButtonText(string agentName,
            Appearance appearance)
        {
            if (Harmony_Patch.Instance.SavePresetButton.IsUnityNull())
            {
                AgentInfoWindow.currentWindow.CreateSavePresetButton();
            }

            _presetLoader.InitializeDefaultCustomPresetFile();
            Harmony_Patch.Instance.SavePresetButton.SetTextColor(Harmony_Patch.Instance.PresetLoader.IsExactPreset(agentName, appearance) ? Color.grey : PresetConstants.PresetTextColor);
        }

        private void SavePresetListToFile([NotNull] PresetList presetList,
            string fileName)
        {
            var jsonData = presetList.ToJson();
            _fileManager.WriteAllText(fileName, jsonData);
        }
    }
}
