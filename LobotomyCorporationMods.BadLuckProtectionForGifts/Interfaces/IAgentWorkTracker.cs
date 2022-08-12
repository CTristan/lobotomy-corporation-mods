using JetBrains.Annotations;

namespace LobotomyCorporationMods.BadLuckProtectionForGifts.Interfaces
{
    public interface IAgentWorkTracker
    {
        float GetLastAgentWorkCountByGift([NotNull] string giftName);
        void IncrementAgentWorkCount([NotNull] string giftName, long agentId, float numberOfTimes = 1f);
        void Load();
        void Reset();
        void Save();
    }
}
