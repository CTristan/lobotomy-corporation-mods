// SPDX-License-Identifier: MIT

using System.Collections.Generic;
using LobotomyCorporationMods.FreeCustomization.Objects;

namespace LobotomyCorporationMods.FreeCustomization.Interfaces
{
    public interface IPresetLoader
    {
        Dictionary<string, PresetData> Presets { get; }
        bool IsPreset(string agentName);
        void LoadPreset(string agentName);
        PresetList LoadSerializablePresetsFromDefaultCustomFile();
    }
}
