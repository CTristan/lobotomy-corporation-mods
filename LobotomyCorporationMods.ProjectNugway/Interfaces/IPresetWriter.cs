// SPDX-License-Identifier: MIT

using Customizing;

namespace LobotomyCorporationMods.ProjectNugway.Interfaces
{
    internal interface IPresetWriter
    {
        void DeletePreset(string presetName);
        void SavePreset();

        void UpdateSavePresetButtonText(string agentName,
            Appearance appearance);
    }
}
