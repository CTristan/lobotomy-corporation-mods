// SPDX-License-Identifier: MIT

using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Attributes;

// ReSharper disable once CheckNamespace
namespace UnityEngine
{
    public static class UnityEngineExtensions
    {
        public static bool IsNullUnityObject([ValidatedNotNull] [CanBeNull] this Object obj)
        {
            return ReferenceEquals(obj, null);
        }
    }
}
