// SPDX-License-Identifier: MIT

namespace LobotomyCorporationMods.Test.Extensions
{
    internal static class StringExtensions
    {
        internal static string ShortenBy(this string value, int lengthToRemove)
        {
            var length = value.Length;

            return value.Remove(length - lengthToRemove);
        }
    }
}
