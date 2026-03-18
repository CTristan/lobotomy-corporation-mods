// SPDX-License-Identifier: MIT

using System.Collections.Generic;
using JetBrains.Annotations;
using Hemocode.Common.Implementations;

namespace Hemocode.Common.Extensions
{
    public static class UseSkillExtensions
    {
        private static CreatureModel GetAbnormality([NotNull] this UseSkill useSkill)
        {
            ThrowHelper.ThrowIfNull(useSkill, nameof(useSkill));

            return useSkill.targetCreature;
        }

        private static List<CreatureEquipmentMakeInfo> GetAbnormalityEgo([NotNull] this UseSkill useSkill)
        {
            return useSkill.GetAbnormalityInfo().equipMakeInfos;
        }

        [CanBeNull]
        private static CreatureEquipmentMakeInfo GetAbnormalityGift([NotNull] this UseSkill useSkill)
        {
            return useSkill.GetAbnormalityEgo().Find(x => x.equipTypeInfo.type == EquipmentTypeInfo.EquipmentType.SPECIAL);
        }

        private static CreatureTypeInfo GetAbnormalityInfo([NotNull] this UseSkill useSkill)
        {
            return useSkill.GetAbnormality().metaInfo;
        }

        [CanBeNull]
        internal static EquipmentTypeInfo GetAbnormalityGiftInfo([NotNull] this UseSkill useSkill)
        {
            return useSkill.GetAbnormalityGift()?.equipTypeInfo;
        }

        internal static AgentModel GetAgent([NotNull] this UseSkill useSkill)
        {
            ThrowHelper.ThrowIfNull(useSkill, nameof(useSkill));

            return useSkill.agent;
        }
    }
}
