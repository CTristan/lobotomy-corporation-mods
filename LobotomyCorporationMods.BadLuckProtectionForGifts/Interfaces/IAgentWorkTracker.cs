// SPDX-License-Identifier: MIT

namespace LobotomyCorporationMods.BadLuckProtectionForGifts.Interfaces
{
    public interface IAgentWorkTracker
    {
        float GetLastAgentWorkCountByGift(string giftName);
        void IncrementAgentWorkCount(string giftName, long agentId);
        void IncrementAgentWorkCount(string giftName, long agentId, float numberOfTimes);
        void Load();
        void Reset();
        void Save();
    }
}
