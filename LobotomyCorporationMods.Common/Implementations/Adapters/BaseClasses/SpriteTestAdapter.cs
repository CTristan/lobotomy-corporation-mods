// SPDX-License-Identifier: MIT

using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Attributes;
using LobotomyCorporationMods.Common.Constants;
using LobotomyCorporationMods.Common.Extensions;
using LobotomyCorporationMods.Common.Interfaces.Adapters.BaseClasses;
using UnityEngine;

namespace LobotomyCorporationMods.Common.Implementations.Adapters.BaseClasses
{
    [AdapterClass]
    [ExcludeFromCodeCoverage(Justification = Messages.UnityCodeCoverageJustification)]
    public sealed class SpriteTestAdapter : TestAdapter<Sprite>, ISpriteTestAdapter
    {
        internal SpriteTestAdapter([NotNull] Sprite gameObject) : base(gameObject)
        {
        }

        public Sprite Create(Texture2D texture,
            Rect rect,
            Vector2 pivot)
        {
            GameObjectInternal = Sprite.Create(texture, rect, pivot);

            return GameObjectInternal;
        }

        public override Sprite GameObject
        {
            get =>
                !GameObjectInternal.IsUnityNull() ? GameObjectInternal : null;
            set =>
                GameObjectInternal = value;
        }
    }
}
