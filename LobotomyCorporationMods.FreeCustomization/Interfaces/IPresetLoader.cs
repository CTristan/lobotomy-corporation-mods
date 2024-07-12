// SPDX-License-Identifier: MIT

using System.Collections.Generic;
using Customizing;
using LobotomyCorporationMods.FreeCustomization.Objects;

namespace LobotomyCorporationMods.FreeCustomization.Interfaces
{
    public interface IPresetLoader
    {
        Dictionary<string, PresetData> Presets { get; }

        bool IsExactPreset(string agentName,
            Appearance appearance);

        bool HasPreset(string agentName);
        void InitializeDefaultCustomPresetFile();
        AgentData LoadPreset(string agentName);
        PresetList LoadPresetsFromDefaultCustomFile();
    }
}
