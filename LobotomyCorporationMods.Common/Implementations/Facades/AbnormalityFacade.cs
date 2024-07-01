// SPDX-License-Identifier: MIT

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Extensions;

// ReSharper disable MemberCanBePrivate.Global
namespace LobotomyCorporationMods.Common.Implementations.Facades
{
    public static class AbnormalityFacade
    {
        public static CreatureModel GetAbnormality([NotNull] this UseSkill useSkill)
        {
            Guard.Against.Null(useSkill, nameof(useSkill));

            return useSkill.targetCreature;
        }

        public static IEnumerable<CreatureEquipmentMakeInfo> GetAbnormalityEgo([NotNull] this UseSkill useSkill)
        {
            return useSkill.GetAbnormalityInfo().equipMakeInfos;
        }

        [CanBeNull]
        public static CreatureEquipmentMakeInfo GetAbnormalityGift([NotNull] this UseSkill useSkill)
        {
            return useSkill.GetAbnormalityEgo().FirstOrDefault(x => x.equipTypeInfo.type == EquipmentTypeInfo.EquipmentType.SPECIAL);
        }

        public static CreatureTypeInfo GetAbnormalityInfo([NotNull] this UseSkill useSkill)
        {
            return useSkill.GetAbnormality().metaInfo;
        }

        public static EquipmentTypeInfo GetAbnormalityGiftInfo([NotNull] this CreatureEquipmentMakeInfo creatureEquipmentMakeInfo)
        {
            Guard.Against.Null(creatureEquipmentMakeInfo, nameof(creatureEquipmentMakeInfo));

            return creatureEquipmentMakeInfo.equipTypeInfo;
        }

        [CanBeNull]
        public static EquipmentTypeInfo GetAbnormalityGiftInfo([NotNull] this UseSkill useSkill)
        {
            return useSkill.GetAbnormalityGift()?.equipTypeInfo;
        }

        [NotNull]
        public static string GetAbnormalityGiftName([NotNull] this CreatureEquipmentMakeInfo creatureEquipmentMakeInfo)
        {
            return creatureEquipmentMakeInfo.GetAbnormalityGiftInfo()?.Name ?? string.Empty;
        }

        [NotNull]
        public static string GetAbnormalityGiftName([NotNull] this UseSkill useSkill)
        {
            return useSkill.GetAbnormalityGiftInfo()?.Name ?? string.Empty;
        }
    }
}
