// SPDX-License-Identifier: MIT

using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Implementations.Adapters.BaseClasses;
using LobotomyCorporationMods.Common.Interfaces.Adapters;
using UnityEngine;

namespace LobotomyCorporationMods.Common.Implementations.Adapters
{
    internal sealed class Texture2dTestAdapter : TextureTestAdapter<Texture2D>, ITexture2dTestAdapter
    {
        internal Texture2dTestAdapter([NotNull] Texture2D gameObject) : base(gameObject)
        {
        }

        public bool LoadImage(byte[] data)
        {
            return GameObject.LoadImage(data);
        }
    }
}
