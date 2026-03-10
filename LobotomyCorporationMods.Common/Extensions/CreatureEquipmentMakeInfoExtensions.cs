// SPDX-License-Identifier: MIT

using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Implementations;

namespace LobotomyCorporationMods.Common.Extensions
{
    public static class CreatureEquipmentMakeInfoExtensions
    {
        internal static EquipmentTypeInfo GetAbnormalityGiftInfo([NotNull] this CreatureEquipmentMakeInfo creatureEquipmentMakeInfo)
        {
            _ = Guard.Against.Null(creatureEquipmentMakeInfo, nameof(creatureEquipmentMakeInfo));

            return creatureEquipmentMakeInfo.equipTypeInfo;
        }
    }
}
