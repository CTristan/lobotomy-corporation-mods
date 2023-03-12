// SPDX-License-Identifier: MIT

#region

using UnityEngine.UI;

#endregion

namespace LobotomyCorporationMods.Common.Interfaces.Adapters
{
    public interface ITextAdapter : IAdapter<Text>, IGraphicAdapter
    {
        new Text GameObject { get; set; }
        new IGameObjectAdapter GameObjectAdapter { get; }
        string Text { get; set; }
    }
}
