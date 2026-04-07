// SPDX-License-Identifier: MIT

using LobotomyCorporation.Mods.Common.Interfaces.Adapters;

namespace Hemocode.WarnWhenAgentWillDieFromWorking.Interfaces
{
    public interface ICreatureData
    {
        long metadataId { get; }
        int qliphothCounter { get; }
        object script { get; }

        bool IsMaxObserved();
        bool IsBeautyAndTheBeastWeakened(IBeautyBeastAnimTestAdapter beautyBeastAnimTestAdapter);
        int GetParasiteTreeNumberOfFlowers(IYggdrasilAnimTestAdapter yggdrasilAnimTestAdapter);
    }
}
