using JetBrains.Annotations;

namespace LobotomyCorporationMods.BadLuckProtectionForGifts.Interfaces
{
    public interface IAgentWorkTracker
    {
        IAgentWorkTracker FromString([NotNull] string trackerData);
        float GetLastAgentWorkCountByGift([NotNull] string giftName);
        void IncrementAgentWorkCount([NotNull] string giftName, long agentId, float numberOfTimes = 1f);
    }
}
