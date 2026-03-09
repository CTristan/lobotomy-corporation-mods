// SPDX-License-Identifier: MIT

using System.Collections.Generic;
using JetBrains.Annotations;

namespace LobotomyCorporationMods.Common.Extensions
{
    internal static class CreatureTypeInfoExtensions
    {
        internal static CreatureEquipmentMakeInfo GetGift([NotNull] this CreatureTypeInfo creatureTypeInfo)
        {
            List<CreatureEquipmentMakeInfo> equipment = creatureTypeInfo.equipMakeInfos;

            return equipment.Find(info => info.equipTypeInfo.type == EquipmentTypeInfo.EquipmentType.SPECIAL);
        }
    }
}
