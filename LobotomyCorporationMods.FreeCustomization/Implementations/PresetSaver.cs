// SPDX-License-Identifier: MIT

using System.IO;
using System.Xml.Serialization;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Interfaces;
using LobotomyCorporationMods.FreeCustomization.Interfaces;
using LobotomyCorporationMods.FreeCustomization.Objects;

namespace LobotomyCorporationMods.FreeCustomization.Implementations
{
    internal sealed class PresetSaver : IPresetSaver
    {
        private const string DefaultFileName = "Presets/presets.xml";
        private readonly IFileManager _fileManager;

        internal PresetSaver(IFileManager fileManager)
        {
            _fileManager = fileManager;
        }

        public void SavePreset(string agentName,
            [NotNull] WorkerSprite.WorkerSprite agentData)
        {
            var data = new Preset
            {
                AgentName = agentName,
                AgentData = agentData,
            };

            using (var writer = new StringWriter())
            {
                var serializer = new XmlSerializer(typeof(Preset));
                serializer.Serialize(writer, data);
                var xmlData = writer.ToString();
                var fileName = _fileManager.GetFile(DefaultFileName);
                _fileManager.WriteAllText(fileName, xmlData);
            }
        }
    }
}
