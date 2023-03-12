// SPDX-License-Identifier: MIT

#region

using System.Diagnostics.CodeAnalysis;
using LobotomyCorporationMods.Common.Attributes;
using LobotomyCorporationMods.Common.Interfaces.Adapters;
using UnityEngine;

#endregion

namespace LobotomyCorporationMods.Common.Implementations.Adapters
{
    [AdapterClass]
    [ExcludeFromCodeCoverage]
    public sealed class Texture2DAdapter : Adapter<Texture2D>, ITexture2DAdapter
    {
        public int Height => GameObject.height;

        public bool LoadImage(byte[] data)
        {
            return GameObject.LoadImage(data);
        }

        public int Width => GameObject.width;
    }
}
