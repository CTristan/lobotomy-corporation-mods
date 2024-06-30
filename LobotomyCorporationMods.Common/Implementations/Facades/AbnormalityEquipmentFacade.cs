// SPDX-License-Identifier: MIT

using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Extensions;

namespace LobotomyCorporationMods.Common.Implementations.Facades
{
    public static class AbnormalityEquipmentFacade
    {
        [CanBeNull]
        public static EquipmentTypeInfo GetGift([NotNull] this CreatureEquipmentMakeInfo creatureEquipmentMakeInfo)
        {
            Guard.Against.Null(creatureEquipmentMakeInfo, nameof(creatureEquipmentMakeInfo));

            return creatureEquipmentMakeInfo.equipTypeInfo;
        }
    }
}
