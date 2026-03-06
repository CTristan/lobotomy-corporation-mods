// SPDX-License-Identifier: MIT

using LobotomyCorporationMods.Common.Interfaces.Adapters;

namespace LobotomyCorporationMods.WarnWhenAgentWillDieFromWorking.Interfaces
{
    internal interface ICreatureData
    {
        long metadataId { get; }
        int qliphothCounter { get; }
        object script { get; }

        bool IsMaxObserved();
        bool IsBeautyAndTheBeastWeakened(IBeautyBeastAnimTestAdapter beautyBeastAnimTestAdapter);
        int GetParasiteTreeNumberOfFlowers(IYggdrasilAnimTestAdapter yggdrasilAnimTestAdapter);
    }
}
