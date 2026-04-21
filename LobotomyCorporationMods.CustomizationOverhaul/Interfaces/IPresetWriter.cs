// SPDX-License-Identifier: MIT

namespace LobotomyCorporationMods.CustomizationOverhaul.Interfaces
{
    public interface IPresetWriter
    {
        void DeletePreset(string presetName);
        void SavePreset();
    }
}
