// SPDX-License-Identifier: MIT

using System;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Attributes;
using LobotomyCorporationMods.Common.Interfaces;

// ReSharper disable once CheckNamespace
namespace LobotomyCorporationMods.FreeCustomization
{
    internal static class GuardExtensions
    {
        /// <summary>
        ///     Returns true if the value is null.
        /// </summary>
        internal static bool IsNull<T>([NotNull] this IGuardClause guardClause, [NotNull] [ValidatedNotNull] [NoEnumeration] T input)
        {
            return ReferenceEquals(input, null);
        }

        [NotNull]
        internal static T Null<T>([NotNull] this IGuardClause guardClause, [NotNull] [ValidatedNotNull] [NoEnumeration] T input, [NotNull] [InvokerParameterName] string parameterName)
        {
            if (ReferenceEquals(input, null))
            {
                throw new ArgumentNullException(parameterName);
            }

            return input;
        }
    }
}
