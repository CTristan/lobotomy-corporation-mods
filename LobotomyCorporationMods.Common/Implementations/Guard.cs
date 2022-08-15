// SPDX-License-Identifier: MIT

using LobotomyCorporationMods.Common.Attributes;

namespace LobotomyCorporationMods.Common.Implementations
{
    public sealed class Guard
    {
        private Guard() { }

        [GuardClause] public static Guard Against { get; } = new Guard();
    }
}
