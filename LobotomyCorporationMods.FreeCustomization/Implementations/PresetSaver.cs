// SPDX-License-Identifier: MIT

using Customizing;
using LobotomyCorporationMods.Common.Interfaces;
using LobotomyCorporationMods.FreeCustomization.Constants;
using LobotomyCorporationMods.FreeCustomization.Interfaces;
using LobotomyCorporationMods.FreeCustomization.Objects;

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
            var data = ReloadDefaultPresetFile();
            data.Presets[agentName] = PresetData.FromAppearanceData(appearanceData);

            var jsonData = data.ToJson();
            var fileName = _fileManager.GetFile(PresetDefaults.DefaultCustomFileName);
            _fileManager.WriteAllText(fileName, jsonData);
        }

        /// <summary>Reloads the default custom preset file from disk in case there are any missing changes before we overwrite the file.</summary>
        private PresetList ReloadDefaultPresetFile()
        {
            return _presetLoader.LoadSerializablePresetsFromDefaultCustomFile();
        }
    }
}
