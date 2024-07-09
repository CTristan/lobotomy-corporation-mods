// SPDX-License-Identifier: MIT

namespace LobotomyCorporationMods.FreeCustomization.Interfaces
{
    public interface IPresetSaver
    {
        void SavePreset(string agentName,
            WorkerSprite.WorkerSprite agentData);
    }
}
