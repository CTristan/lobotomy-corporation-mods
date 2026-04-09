// SPDX-License-Identifier: MIT

using System.Collections.Generic;
using JetBrains.Annotations;

namespace LobotomyCorporationMods.Common.Implementations
{
    internal static class DefaultLocalizedValues
    {
        private static readonly Dictionary<string, string> s_defaultLocalizedValuesDictionary =
            new Dictionary<string, string>();

        internal static void AddOrOverwriteDefaultLocalizedValue([NotNull] string key, string value)
        {
            s_defaultLocalizedValuesDictionary[key] = value;
        }

        internal static bool TryGetDefaultLocalizedValue(
            [NotNull] string key,
            [CanBeNull] out string value
        )
        {
            return s_defaultLocalizedValuesDictionary.TryGetValue(key, out value);
        }
    }
}
