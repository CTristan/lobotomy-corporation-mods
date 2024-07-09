// SPDX-License-Identifier: MIT

namespace LobotomyCorporationMods.FreeCustomization.Interfaces
{
    public interface IPresetLoader
    {
        bool IsPreset(string agentName);
        void LoadPreset(string agentName);
    }
}
