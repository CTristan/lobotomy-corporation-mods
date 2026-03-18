// SPDX-License-Identifier: MIT

#region

using Hemocode.BadLuckProtectionForGifts.Interfaces;

#endregion

namespace Hemocode.BadLuckProtectionForGifts.Implementations
{
    public sealed class Agent : IAgent
    {
        private readonly long _id;
        private float _workCount;

        internal Agent(long id)
        {
            _id = id;
            _workCount = 0f;
        }

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
