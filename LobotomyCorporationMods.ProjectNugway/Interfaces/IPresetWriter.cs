// SPDX-License-Identifier: MIT

namespace LobotomyCorporationMods.ProjectNugway.Interfaces
{
    public interface IPresetWriter
    {
        void DeletePreset(string presetName);
        void SavePreset();
    }
}
