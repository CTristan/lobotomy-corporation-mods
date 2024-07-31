// SPDX-License-Identifier: MIT

using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Attributes;

namespace LobotomyCorporationMods.Common.Extensions
{
    public static class GameManagerExtensions
    {
        /// <summary>Checks if the GameManger is actually null and not just "Unity null".</summary>
        /// <param name="value">The object to check for null.</param>
        /// <returns>True if the object is null; otherwise, false.</returns>
        [ContractAnnotation("null => true")]
        public static bool IsNull([CanBeNull] [ValidatedNotNull] this GameManager value)
        {
#pragma warning disable IDE0041
            return ReferenceEquals(value, null);
#pragma warning restore IDE0041
        }
    }
}
