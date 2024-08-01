// SPDX-License-Identifier: MIT

#region

using JetBrains.Annotations;
using UnityEngine;

#endregion

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
