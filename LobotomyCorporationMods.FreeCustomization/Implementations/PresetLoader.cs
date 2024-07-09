// SPDX-License-Identifier: MIT

using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using LobotomyCorporationMods.Common.Extensions;
using LobotomyCorporationMods.Common.Interfaces;
using LobotomyCorporationMods.FreeCustomization.Interfaces;
using LobotomyCorporationMods.FreeCustomization.Objects;

namespace LobotomyCorporationMods.FreeCustomization.Implementations
{
    internal sealed class PresetLoader : IPresetLoader
    {
        private const string DefaultFileName = "Presets/presets.xml";
        private readonly IFileManager _fileManager;
        private readonly Dictionary<string, Preset> _presets = new Dictionary<string, Preset>();

        internal PresetLoader(IFileManager fileManager)
        {
            _fileManager = fileManager;
            InitializeAllPresetFiles();
        }

        public bool IsPreset(string agentName)
        {
            return agentName.IsNotNull();
        }

        public void LoadPreset(string agentName)
        {
        }

        private void InitializeAllPresetFiles()
        {
            InitializePresetsFromFile(DefaultFileName);
        }

        private void InitializePresetsFromFile(string fileName)
        {
            var file = _fileManager.GetFile(fileName);
            var xmlData = _fileManager.ReadAllText(file, false);

            if (string.IsNullOrEmpty(xmlData))
            {
                return;
            }

            var xmlSettings = new XmlReaderSettings
            {
                ProhibitDtd = true,
            };

            using (var reader = XmlReader.Create(xmlData, xmlSettings))
            {
                var xmlSerializer = new XmlSerializer(typeof(PresetList));
                var presetData = xmlSerializer.Deserialize(reader) as PresetList;
                foreach (var preset in presetData.Presets)
                {
                    _presets[preset.AgentName] = preset;
                }
            }
        }
    }
}
