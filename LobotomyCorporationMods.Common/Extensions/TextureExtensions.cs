// SPDX-License-Identifier: MIT

using UnityEngine;

namespace LobotomyCorporationMods.Common.Extensions
{
    internal static class TextureExtensions
    {
        internal static bool IsUnityNull(this Texture texture)
        {
            return !texture;
        }
    }
}
