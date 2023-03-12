// SPDX-License-Identifier: MIT

#region

using UnityEngine;
using UnityEngine.UI;

#endregion

namespace LobotomyCorporationMods.Common.Interfaces.Adapters
{
    public interface IImageAdapter : IAdapter<Image>, IGraphicAdapter
    {
        Color Color { get; set; }
        new Image GameObject { get; set; }
        new IGameObjectAdapter GameObjectAdapter { get; }
        Sprite Sprite { get; set; }
    }
}
