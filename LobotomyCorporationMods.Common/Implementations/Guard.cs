// SPDX-License-Identifier: MIT

using LobotomyCorporationMods.Common.Interfaces;

namespace LobotomyCorporationMods.Common.Implementations
{
    public sealed class Guard : IGuardClause
    {
        private Guard() { }
        public static IGuardClause Against { get; } = new Guard();
    }
}
