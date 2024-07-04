// SPDX-License-Identifier: MIT

using System;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Attributes;
using LobotomyCorporationMods.Common.Implementations;

namespace LobotomyCorporationMods.Common.Extensions
{
    public static class ObjectExtensions
    {
        /// <summary>
        /// Ensure that a value is not null, and if it.IsNull(), return the result of the specified default method.
        /// </summary>
        /// <remarks>
        /// This method is used to ensure that a value is not null. If the value.IsNull(), the specified default method will be called to provide a default value.
        /// The default method should return a value of the same type as the value being checked.
        /// </remarks>
        /// <typeparam name="T">The type of the value being checked.</typeparam>
        /// <param name="value">The value to check for null.</param>
        /// <param name="defaultMethod">The method to call to provide a default value if the value.IsNull().</param>
        /// <returns>The original value if it is not null, or the result of the default method if the value.IsNull().</returns>
        public static T EnsureNotNullWithMethod<T>([CanBeNull] this T value,
            [NotNull] Func<T> defaultMethod) where T : class
        {
            Guard.Against.Null(defaultMethod, nameof(defaultMethod));

            return value ?? defaultMethod();
        }

        [ContractAnnotation("null => true")]
        public static bool IsNull([CanBeNull] [ValidatedNotNull] this object value)
        {
#pragma warning disable IDE0041
            return ReferenceEquals(value, null);
#pragma warning restore IDE0041
        }

        [ContractAnnotation("null => false")]
        public static bool IsNotNull([CanBeNull] [ValidatedNotNull] this object value)
        {
            return !value.IsNull();
        }
    }
}
