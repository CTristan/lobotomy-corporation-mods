// SPDX-License-Identifier: MIT

#region

using JetBrains.Annotations;

#endregion

namespace LobotomyCorporationMods.BadLuckProtectionForGifts.Extensions
{
    internal static class UseSkillExtensions
    {
        [CanBeNull]
        internal static CreatureEquipmentMakeInfo GetCreatureEquipmentMakeInfo([NotNull] this UseSkill instance)
        {
            var equipmentMakeInfo = instance.targetCreature?.metaInfo?.equipMakeInfos?.Find(static x => x?.equipTypeInfo?.type == EquipmentTypeInfo.EquipmentType.SPECIAL);

            return equipmentMakeInfo;
        }
    }
}
