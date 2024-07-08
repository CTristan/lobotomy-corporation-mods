// SPDX-License-Identifier: MIT

using UnityEngine;

namespace LobotomyCorporationMods.Common.Extensions
{
    internal static class SpriteExtensions
    {
        internal static bool IsUnityNull(this Sprite sprite)
        {
            return !sprite;
        }
    }
}
