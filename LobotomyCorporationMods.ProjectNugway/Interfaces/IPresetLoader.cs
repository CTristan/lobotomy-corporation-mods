// SPDX-License-Identifier: MIT

using System.Collections.Generic;
using Customizing;
using LobotomyCorporationMods.ProjectNugway.Objects;

namespace LobotomyCorporationMods.ProjectNugway.Interfaces
{
    public interface IPresetLoader
    {
        Dictionary<string, PresetData> Presets { get; }
        IEnumerable<string> FindAllPresetFiles();
        bool HasPreset(string agentName);
        void InitializeDefaultCustomPresetFile();
        AgentData LoadPreset(string agentName);
        PresetList LoadPresetsFromCustomFile(string fileName = null);
        void ReloadPresetsFromFiles();

        bool IsExactPreset(string agentName,
            Appearance appearance);
    }
}
