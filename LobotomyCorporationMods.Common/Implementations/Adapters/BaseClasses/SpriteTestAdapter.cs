// SPDX-License-Identifier: MIT

using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Attributes.ValidCodeCoverageExceptionAttributes;
using LobotomyCorporationMods.Common.Constants;
using LobotomyCorporationMods.Common.Interfaces.Adapters.BaseClasses;
using UnityEngine;

namespace LobotomyCorporationMods.Common.Implementations.Adapters.BaseClasses
{
    [AdapterClass]
    [ExcludeFromCodeCoverage(Justification = Messages.UnityCodeCoverageJustification)]
    internal sealed class SpriteTestAdapter : TestAdapter<Sprite>, ISpriteTestAdapter
    {
        internal SpriteTestAdapter([NotNull] Sprite gameObject) : base(gameObject)
        {
        }

        public Sprite Create(Texture2D texture,
            Rect rect,
            Vector2 pivot)
        {
            _gameObject = Sprite.Create(texture, rect, pivot);

            return _gameObject;
        }
    }
}
