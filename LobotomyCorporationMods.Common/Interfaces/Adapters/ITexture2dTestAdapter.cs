// SPDX-License-Identifier: MIT

using LobotomyCorporationMods.Common.Interfaces.Adapters.BaseClasses;
using UnityEngine;

namespace LobotomyCorporationMods.Common.Interfaces.Adapters
{
    public interface ITexture2dTestAdapter : ITextureTestAdapter<Texture2D>
    {
        bool LoadImage(byte[] data);
    }
}
