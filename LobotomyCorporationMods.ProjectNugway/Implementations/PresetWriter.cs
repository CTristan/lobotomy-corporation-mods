// SPDX-License-Identifier: MIT

using Customizing;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Interfaces;
using LobotomyCorporationMods.ProjectNugway.Constants;
using LobotomyCorporationMods.ProjectNugway.Interfaces;
using LobotomyCorporationMods.ProjectNugway.Objects;

namespace LobotomyCorporationMods.ProjectNugway.Implementations
{
    internal sealed class PresetWriter : IPresetWriter
    {
        private readonly IFileManager _fileManager;
        private readonly IPresetLoader _presetLoader;
        private readonly IUiController _uiController;

        internal PresetWriter([NotNull] IFileManager fileManager,
            IPresetLoader presetLoader,
            IUiController uiController)
        {
            _fileManager = fileManager;
            _presetLoader = presetLoader;
            _uiController = uiController;
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
            var appearanceUi = CustomizingWindow.CurrentWindow.appearanceUI;
            var agentName = appearanceUi.NameInput.text;
            if (string.IsNullOrEmpty(agentName))
            {
                agentName = appearanceUi.copied.CustomName;
            }

            var appearanceData = appearanceUi.copied.appearance;

            // Reload the default custom preset file in case there are any missing changes before we overwrite the file.
            var data = _presetLoader.LoadPresetsFromCustomFile();
            data.Presets[agentName] = PresetData.FromAppearanceData(appearanceData);

            SavePresetListToFile(data, _fileManager.GetFile(PresetConstants.CustomFileName));

            _uiController.UpdateSavePresetButtonText(agentName, appearanceData);
        }

        private void SavePresetListToFile([NotNull] PresetList presetList,
            string fileName)
        {
            var jsonData = presetList.ToJson();
            _fileManager.WriteAllText(fileName, jsonData);
        }
    }
}
