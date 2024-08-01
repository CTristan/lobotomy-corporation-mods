// SPDX-License-Identifier: MIT

#region

using System;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Implementations;

#endregion

namespace LobotomyCorporationMods.Common.Extensions
{
    public static class ObjectExtensions
    {
        /// <summary>Ensure that a value is not null, and if it == null, return the result of the specified default method.</summary>
        /// <remarks>
        ///     This method is used to ensure that a value is not null. If the value == null, the specified default method will be called to provide a default value. The default method
        ///     should return a value of the same type as the value being checked.
        /// </remarks>
        /// <typeparam name="T">The type of the value being checked.</typeparam>
        /// <param name="value">The value to check for null.</param>
        /// <param name="defaultMethod">The method to call to provide a default value if the value == null.</param>
        /// <returns>The original value if it is not null, or the result of the default method if the value == null.</returns>
        [NotNull]
        public static T EnsureNotNullWithMethod<T>([CanBeNull] this T value,
            [NotNull] Func<T> defaultMethod) where T : class
        {
            Guard.Against.Null(defaultMethod, nameof(defaultMethod));

            return value ?? defaultMethod();
        }
    }
}
