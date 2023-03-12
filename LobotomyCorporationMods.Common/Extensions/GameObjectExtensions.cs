// SPDX-License-Identifier: MIT

#region

using UnityEngine;

#endregion

namespace LobotomyCorporationMods.Common.Extensions
{
    public static class GameObjectExtensions
    {
        /// <summary>
        ///     Unity overrides the equality operators to return a lifetime check rather than an actual null check. Because of
        ///     this, a simple null check won't work for determining if an object is actually active, as an object can be not null
        ///     but still be considered null by Unity if it is not active.
        ///     To determine if a non-null object is active and populated, we have to perform a boolean check of the object.
        ///     For more information:
        ///     https://github.com/JetBrains/resharper-unity/wiki/Possible-unintended-bypass-of-lifetime-check-of-underlying-Unity-engine-object
        /// </summary>
        public static bool IsUnityNull(this GameObject gameObject)
        {
            return !gameObject;
        }
    }
}
