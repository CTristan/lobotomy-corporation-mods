// SPDX-License-Identifier: MIT

using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Interfaces.Adapters.BaseClasses;
using UnityEngine;

namespace LobotomyCorporationMods.Common.Implementations.Adapters.BaseClasses
{
    internal class TextureTestAdapter<T> : TestAdapter<T>, ITextureTestAdapter<T> where T : Texture
    {
        protected TextureTestAdapter([NotNull] T gameObject) : base(gameObject)
        {
        }

        public int Width => GameObject.width;
        public int Height => GameObject.height;
    }
}
