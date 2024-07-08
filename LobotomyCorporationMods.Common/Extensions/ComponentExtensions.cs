// SPDX-License-Identifier: MIT

using UnityEngine;

namespace LobotomyCorporationMods.Common.Extensions
{
    internal static class ComponentExtensions
    {
        internal static bool IsUnityNull(this Component component)
        {
            return !component;
        }
    }
}
