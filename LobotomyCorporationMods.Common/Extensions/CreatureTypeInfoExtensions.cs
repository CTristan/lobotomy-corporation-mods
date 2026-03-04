// SPDX-License-Identifier: MIT

using JetBrains.Annotations;

namespace LobotomyCorporationMods.Common.Extensions
{
    internal static class CreatureTypeInfoExtensions
    {
        internal static CreatureEquipmentMakeInfo GetGift([NotNull] this CreatureTypeInfo creatureTypeInfo)
        {
            var equipment = creatureTypeInfo.equipMakeInfos;

            return equipment.Find(info => info.equipTypeInfo.type == EquipmentTypeInfo.EquipmentType.SPECIAL);
        }
    }
}
