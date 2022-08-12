// SPDX-License-Identifier: MIT

using JetBrains.Annotations;

namespace LobotomyCorporationMods.BadLuckProtectionForGifts.Interfaces
{
    public interface IAgentWorkTracker
    {
        float GetLastAgentWorkCountByGift([NotNull] string giftName);
        void IncrementAgentWorkCount([NotNull] string giftName, long agentId);
        void IncrementAgentWorkCount([NotNull] string giftName, long agentId, float numberOfTimes);
        void Load();
        void Reset();
        void Save();
    }
}
