// SPDX-License-Identifier: MIT

using UnityEngine;

namespace LobotomyCorporationMods.Common.Extensions
{
    internal static class GameObjectTestExtensions
    {
        internal static bool IsUnityNull(this GameObject gameObject)
        {
            return !gameObject;
        }
    }
}
