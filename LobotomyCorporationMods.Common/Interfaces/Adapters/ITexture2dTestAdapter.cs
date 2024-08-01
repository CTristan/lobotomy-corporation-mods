// SPDX-License-Identifier: MIT

#region

using LobotomyCorporationMods.Common.Interfaces.Adapters.BaseClasses;
using UnityEngine;

#endregion

namespace LobotomyCorporationMods.Common.Interfaces.Adapters
{
    public interface ITexture2dTestAdapter : ITextureTestAdapter<Texture2D>
    {
        bool LoadImage(byte[] data);
    }
}
