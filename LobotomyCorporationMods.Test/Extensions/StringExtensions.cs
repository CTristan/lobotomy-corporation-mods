// SPDX-License-Identifier: MIT

using JetBrains.Annotations;

namespace LobotomyCorporationMods.Test.Extensions
{
    internal static class StringExtensions
    {
        [NotNull]
        internal static string ShortenBy([NotNull] this string value,
            int lengthToRemove)
        {
            int length = value.Length;

            return value[..(length - lengthToRemove)];
        }
    }
}
