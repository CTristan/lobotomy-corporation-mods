// SPDX-License-Identifier: MIT

using System;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Attributes;
using LobotomyCorporationMods.Common.Interfaces;

namespace LobotomyCorporationMods.Common.Implementations
{
    public sealed class Guard : IGuard
    {
        private Guard() { }

        [GuardClause] public static IGuard Against { get; } = new Guard();

        [NotNull]
        public T Null<T>([ValidatedNotNull] [NoEnumeration] T input, [InvokerParameterName] string parameterName)
        {
            if (input is null)
            {
                throw new ArgumentNullException(parameterName);
            }

            return input;
        }
    }
}
