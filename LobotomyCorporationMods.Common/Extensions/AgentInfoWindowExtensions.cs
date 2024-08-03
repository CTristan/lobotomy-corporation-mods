// SPDX-License-Identifier: MIT

#region

using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Attributes;

#endregion

namespace LobotomyCorporationMods.Common.Extensions
{
    public static class AgentInfoWindowExtensions
    {
        /// <summary>Checks if the AgentInfoWindow is actually null and not just "Unity null".</summary>
        /// <param name="value">The object to check for null.</param>
        /// <returns>True if the object is null; otherwise, false.</returns>
        /// <remarks>
        ///     AgentInfoWindow is a static game object instance, so we're never going to re-initialize it. This also lets us run unit tests involving AgentInfoWindow because it will
        ///     never be enabled when running unit tests.
        /// </remarks>
        [ContractAnnotation("null => true")]
        public static bool IsNull([CanBeNull] [ValidatedNotNull] this AgentInfoWindow value)
        {
#pragma warning disable IDE0041
            return ReferenceEquals(value, null);
#pragma warning restore IDE0041
        }
    }
}
