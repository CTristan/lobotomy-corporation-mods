// SPDX-License-Identifier: MIT

using Hemocode.Common.Interfaces.Adapters.BaseClasses;
using UnityEngine;

namespace Hemocode.Common.Interfaces.Adapters
{
    public interface ITexture2dTestAdapter : ITextureTestAdapter<Texture2D>
    {
        bool LoadImage(byte[] data);
    }
}
