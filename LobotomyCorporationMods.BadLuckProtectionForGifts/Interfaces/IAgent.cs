// SPDX-License-Identifier: MIT

namespace Hemocode.BadLuckProtectionForGifts.Interfaces
{
    public interface IAgent
    {
        long GetId();
        float GetWorkCount();
        void IncrementWorkCount(float numberOfTimes);
    }
}
