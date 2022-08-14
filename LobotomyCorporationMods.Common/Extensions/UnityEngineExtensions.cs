// SPDX-License-Identifier: MIT

using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Attributes;

// ReSharper disable once CheckNamespace
namespace UnityEngine
{
    public static class UnityEngineExtensions
    {
        /// <summary>
        ///     Needed because Unity overrides the equality operators, so obj == null will always return true while testing.
        ///     https://github.com/JetBrains/resharper-unity/wiki/Possible-unintended-bypass-of-lifetime-check-of-underlying-Unity-engine-object
        /// </summary>
        public static bool IsNull([ValidatedNotNull] [CanBeNull] this Object obj)
        {
            return ReferenceEquals(obj, null);
        }
    }
}
