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
    internal class TextureTestAdapter<T> : TestAdapter<T>, ITextureTestAdapter<T> where T : Texture
    {
        protected TextureTestAdapter([NotNull] T gameObject) : base(gameObject)
        {
        }

        public int Width => _gameObject.width;
        public int Height => _gameObject.height;
    }
}
