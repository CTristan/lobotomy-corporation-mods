// SPDX-License-Identifier: MIT

using LobotomyCorporationMods.Common.Interfaces.Adapters.BaseClasses;

namespace LobotomyCorporationMods.Common.Interfaces.Adapters
{
    public interface IBeautyBeastAnimTestAdapter : IComponentTestAdapter<BeautyBeastAnim>
    {
        int State { get; }
    }
}
