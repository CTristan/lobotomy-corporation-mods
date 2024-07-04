// SPDX-License-Identifier: MIT

using System;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Attributes;
using LobotomyCorporationMods.Common.Implementations;

namespace LobotomyCorporationMods.Common.Extensions
{
    public static class GuardExtensions
    {
        [NotNull]
        public static T Null<T>([NotNull] [GuardClause] this Guard guardClause,
            [NotNull] [ValidatedNotNull] [NoEnumeration]
            T input,
            [NotNull] [InvokerParameterName] string parameterName)
        {
            if (input.IsNull())
            {
                throw new ArgumentNullException(parameterName);
            }

            return input;
        }
    }
}
