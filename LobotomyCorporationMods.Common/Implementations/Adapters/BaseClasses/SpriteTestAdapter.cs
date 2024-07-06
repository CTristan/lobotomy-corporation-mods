// SPDX-License-Identifier: MIT

using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Interfaces.Adapters.BaseClasses;
using UnityEngine;

namespace LobotomyCorporationMods.Common.Implementations.Adapters.BaseClasses
{
    internal sealed class SpriteTestAdapter : TestAdapter<Sprite>, ISpriteTestAdapter
    {
        internal SpriteTestAdapter([NotNull] Sprite gameObject) : base(gameObject)
        {
        }

        public Sprite Create(Texture2D texture,
            Rect rect,
            Vector2 pivot)
        {
            return Sprite.Create(texture, rect, pivot);
        }
    }
}
