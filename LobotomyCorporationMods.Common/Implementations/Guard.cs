// SPDX-License-Identifier: MIT

#region

using System;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Attributes;

#endregion

namespace LobotomyCorporationMods.Common.Implementations
{
    public static class Guard
    {
        [NotNull]
        public static void NotNull<T>([ValidatedNotNull] this T value, string name) where T : class
        {
            if (value is null)
            {
                throw new ArgumentNullException(name);
            }
        }
    }
}
