// SPDX-License-Identifier: MIT

using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Enums;
using LobotomyCorporationMods.Common.Implementations;

namespace LobotomyCorporationMods.Common.Extensions
{
    public static class UnitModelExtensions
    {
        public static bool HasEquipment([NotNull] this UnitModel agent, EquipmentId equipmentId)
        {
            Guard.Against.Null(agent, nameof(agent));
            Guard.Against.Null(equipmentId, nameof(equipmentId));

            return agent.HasEquipment((int)equipmentId);
        }
    }
}
