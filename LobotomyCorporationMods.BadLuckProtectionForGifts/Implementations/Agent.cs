using LobotomyCorporationMods.BadLuckProtectionForGifts.Interfaces;

namespace LobotomyCorporationMods.BadLuckProtectionForGifts.Implementations
{
    internal sealed class Agent : IAgent
    {
        public Agent(long id)
        {
            _id = id;
            _workCount = 0f;
        }

        private readonly long _id;
        private float _workCount;

        public long GetId()
        {
            return _id;
        }

        public float GetWorkCount()
        {
            return _workCount;
        }

        public void IncrementWorkCount(float numberOfTimes)
        {
            _workCount += numberOfTimes;
        }
    }
}
