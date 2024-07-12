// SPDX-License-Identifier: MIT

using Customizing;

namespace LobotomyCorporationMods.FreeCustomization.Interfaces
{
    public interface IPresetSaver
    {
        void SavePreset();

        void UpdateSavePresetButtonText(string agentName,
            Appearance appearance);
    }
}
