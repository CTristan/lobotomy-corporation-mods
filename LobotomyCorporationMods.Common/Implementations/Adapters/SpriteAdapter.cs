// SPDX-License-Identifier: MIT

#region

using System.Diagnostics.CodeAnalysis;
using LobotomyCorporationMods.Common.Attributes;
using LobotomyCorporationMods.Common.Interfaces.Adapters;
using UnityEngine;

#endregion

namespace LobotomyCorporationMods.Common.Implementations.Adapters
{
    [AdapterClass]
    [ExcludeFromCodeCoverage]
    public sealed class SpriteAdapter : Adapter<Sprite>, ISpriteAdapter
    {
        public Sprite Create(Texture2D texture, Rect rect, Vector2 pivot)
        {
            return Sprite.Create(texture, rect, pivot);
        }
    }
}
