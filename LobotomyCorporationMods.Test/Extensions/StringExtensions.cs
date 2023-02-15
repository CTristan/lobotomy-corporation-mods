// SPDX-License-Identifier: MIT

#region

using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Extensions;
using LobotomyCorporationMods.Common.Implementations;

#endregion

namespace LobotomyCorporationMods.Test.Extensions
{
    internal static class StringExtensions
    {
        [NotNull]
        internal static string ShortenBy([NotNull] this string value, int lengthToRemove)
        {
            Guard.Against.Null(value, nameof(value));

            var length = value.Length;

            return value.Remove(length - lengthToRemove);
        }
    }
}
