// SPDX-License-Identifier: MIT

#region

using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Implementations;

#endregion

namespace LobotomyCorporationMods.Common.Extensions
{
    internal static class CreatureEquipmentMakeInfoExtensions
    {
        internal static EquipmentTypeInfo GetAbnormalityGiftInfo([NotNull] this CreatureEquipmentMakeInfo creatureEquipmentMakeInfo)
        {
            Guard.Against.Null(creatureEquipmentMakeInfo, nameof(creatureEquipmentMakeInfo));

            return creatureEquipmentMakeInfo.equipTypeInfo;
        }
    }
}
