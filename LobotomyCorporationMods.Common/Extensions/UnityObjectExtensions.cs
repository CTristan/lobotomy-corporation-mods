// SPDX-License-Identifier: MIT

using JetBrains.Annotations;
using UnityEngine;

namespace LobotomyCorporationMods.Common.Extensions
{
    public static class UnityObjectExtensions
    {
        [NotNull]
        internal static string GetName([NotNull] this Object unityObject)
        {
            return unityObject != null ? unityObject.name : string.Empty;
        }
    }
}
