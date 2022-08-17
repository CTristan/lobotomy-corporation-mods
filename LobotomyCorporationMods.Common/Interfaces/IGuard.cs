// SPDX-License-Identifier: MIT

using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Attributes;

namespace LobotomyCorporationMods.Common.Interfaces
{
    public interface IGuard
    {
        public T Null<T>([CanBeNull] [ValidatedNotNull] [NoEnumeration] T input, [NotNull] [InvokerParameterName] string parameterName);
    }
}
