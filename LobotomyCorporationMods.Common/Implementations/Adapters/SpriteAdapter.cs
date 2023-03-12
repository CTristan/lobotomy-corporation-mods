// SPDX-License-Identifier: MIT

#region

using LobotomyCorporationMods.Common.Interfaces.Adapters;
using UnityEngine;

#endregion

namespace LobotomyCorporationMods.Common.Implementations.Adapters
{
    public class SpriteAdapter : Adapter<Sprite>, ISpriteAdapter
    {
        public Sprite Create(Texture2D texture, Rect rect, Vector2 pivot)
        {
            return Sprite.Create(texture, rect, pivot);
        }
    }
}
