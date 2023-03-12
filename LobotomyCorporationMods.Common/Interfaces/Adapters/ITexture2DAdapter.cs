// SPDX-License-Identifier: MIT

#region

using UnityEngine;

#endregion

namespace LobotomyCorporationMods.Common.Interfaces.Adapters
{
    public interface ITexture2DAdapter : IAdapter<Texture2D>
    {
        int Height { get; }
        int Width { get; }
        bool LoadImage(byte[] data);
    }
}
