// SPDX-License-Identifier: MIT

using Customizing;

namespace LobotomyCorporationMods.FreeCustomization.Interfaces
{
    public interface IPresetSaver
    {
        void SavePreset(string agentName,
            Appearance agentData);
    }
}
