// SPDX-License-Identifier: MIT

namespace LobotomyCorporationMods.BadLuckProtectionForGifts.Interfaces
{
    public interface IAgent
    {
        long GetId();
        float GetWorkCount();
        void IncrementWorkCount(float numberOfTimes);
    }
}
