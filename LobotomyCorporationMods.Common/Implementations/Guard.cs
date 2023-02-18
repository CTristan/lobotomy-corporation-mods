// SPDX-License-Identifier: MIT

#region

using LobotomyCorporationMods.Common.Attributes;

#endregion

namespace LobotomyCorporationMods.Common.Implementations
{
    public sealed class Guard
    {
        private Guard() {}

        [GuardClause]
        public static Guard Against { get; } = new();
    }
}
