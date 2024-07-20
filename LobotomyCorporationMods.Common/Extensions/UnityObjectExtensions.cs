// SPDX-License-Identifier: MIT

using JetBrains.Annotations;
using UnityEngine;

namespace LobotomyCorporationMods.Common.Extensions
{
    public static class UnityObjectExtensions
    {
        internal static string GetName([NotNull] this Object unityObject)
        {
            return unityObject.IsNotNull() ? unityObject.name : string.Empty;
        }

        public static bool IsUnityNull([CanBeNull] this Object unityObject)
        {
            return unityObject == null;
        }
    }
}
