// SPDX-License-Identifier: MIT

#region

using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Implementations;

#endregion

namespace LobotomyCorporationMods.Test.Extensions
{
    internal static class StringExtensions
    {
        [NotNull]
        internal static string ShortenBy([NotNull] this string value, int lengthToRemove)
        {
            value.NotNull(nameof(value));

            var length = value.Length;

            return value.Remove(length - lengthToRemove);
        }
    }
}
