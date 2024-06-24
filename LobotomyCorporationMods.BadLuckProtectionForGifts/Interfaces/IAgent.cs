// SPDX-License-Identifier: MIT

namespace LobotomyCorporationMods.BadLuckProtectionForGifts.Interfaces
{
    internal interface IAgent
    {
        long GetId();
        float GetWorkCount();
        void IncrementWorkCount(float numberOfTimes);
    }
}
