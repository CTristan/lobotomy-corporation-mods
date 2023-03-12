// SPDX-License-Identifier: MIT

#region

using UnityEngine;

#endregion

namespace LobotomyCorporationMods.Common.Interfaces.Adapters
{
    public interface ISpriteAdapter : IAdapter<Sprite>
    {
        Sprite Create(Texture2D texture, Rect rect, Vector2 pivot);
    }
}
