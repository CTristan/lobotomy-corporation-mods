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
        /// <remarks>
        ///     GameManger is a static game object instance, so we're never going to re-initialize it. This also lets us run unit tests involving GameManager because it will never be
        ///     enabled when running unit tests.
        /// </remarks>
        [ContractAnnotation("null => true")]
        public static bool IsNull([CanBeNull] [ValidatedNotNull] this GameManager value)
        {
#pragma warning disable IDE0041
            return ReferenceEquals(value, null);
#pragma warning restore IDE0041
        }
    }
}
