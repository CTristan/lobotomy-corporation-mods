// SPDX-License-Identifier: MIT

using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Attributes;
using LobotomyCorporationMods.Common.Constants;
using LobotomyCorporationMods.Common.Implementations.Adapters.BaseClasses;
using LobotomyCorporationMods.Common.Interfaces.Adapters;
using UnityEngine;

namespace LobotomyCorporationMods.Common.Implementations.Adapters
{
    [AdapterClass]
    [ExcludeFromCodeCoverage(Justification = Messages.UnityCodeCoverageJustification)]
    internal sealed class Texture2dTestAdapter : TextureTestAdapter<Texture2D>, ITexture2dTestAdapter
    {
        internal Texture2dTestAdapter([NotNull] Texture2D gameObject) : base(gameObject)
        {
        }

        public bool LoadImage(byte[] data)
        {
            return _gameObject.LoadImage(data);
        }
    }
}
