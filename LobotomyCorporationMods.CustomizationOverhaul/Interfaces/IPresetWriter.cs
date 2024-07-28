// SPDX-License-Identifier: MIT

using Customizing;

namespace LobotomyCorporationMods.CustomizationOverhaul.Interfaces
{
    internal interface IPresetWriter
    {
        void DeletePreset(string presetName);
        void SavePreset();

        void UpdateSavePresetButtonText(string agentName,
            Appearance appearance);
    }
}
