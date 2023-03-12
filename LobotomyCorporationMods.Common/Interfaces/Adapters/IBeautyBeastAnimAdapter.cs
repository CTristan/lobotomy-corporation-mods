// SPDX-License-Identifier: MIT

namespace LobotomyCorporationMods.Common.Interfaces.Adapters
{
    public interface IBeautyBeastAnimAdapter : IAdapter<BeautyBeastAnim>, IComponentAdapter
    {
        new BeautyBeastAnim GameObject { get; set; }
        new IGameObjectAdapter GameObjectAdapter { get; }
        int State { get; }
    }
}
