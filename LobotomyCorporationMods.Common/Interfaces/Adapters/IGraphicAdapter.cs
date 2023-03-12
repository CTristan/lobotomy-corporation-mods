// SPDX-License-Identifier: MIT

#region

using UnityEngine.UI;

#endregion

namespace LobotomyCorporationMods.Common.Interfaces.Adapters
{
    public interface IGraphicAdapter : IAdapter<Graphic>, IComponentAdapter
    {
        new Graphic GameObject { get; set; }
        new IGameObjectAdapter GameObjectAdapter { get; }
    }
}
