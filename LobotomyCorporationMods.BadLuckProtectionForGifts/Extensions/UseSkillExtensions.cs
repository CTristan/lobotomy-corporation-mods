// SPDX-License-Identifier: MIT

namespace LobotomyCorporationMods.BadLuckProtectionForGifts.Extensions
{
    internal static class UseSkillExtensions
    {
        internal static CreatureEquipmentMakeInfo GetCreatureEquipmentMakeInfo(this UseSkill instance)
        {
            var equipmentMakeInfo = instance.targetCreature.metaInfo.equipMakeInfos.Find(x =>
                x.equipTypeInfo.type == EquipmentTypeInfo.EquipmentType.SPECIAL);

            return equipmentMakeInfo;
        }
    }
}
