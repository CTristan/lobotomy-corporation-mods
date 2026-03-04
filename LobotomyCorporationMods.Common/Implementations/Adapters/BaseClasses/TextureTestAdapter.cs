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
    internal class TextureTestAdapter<T> : TestAdapter<T>, ITextureTestAdapter<T> where T : Texture
    {
        protected TextureTestAdapter([NotNull] T gameObject) : base(gameObject)
        {
        }

        public int Width =>
            _gameObject.width;

        public int Height =>
            _gameObject.height;

        public override T GameObject
        {
            get =>
                !_gameObject.IsUnityNull() ? _gameObject : null;
            set =>
                _gameObject = value;
        }
    }
}