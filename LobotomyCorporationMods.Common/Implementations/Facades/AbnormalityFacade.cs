// SPDX-License-Identifier: MIT

using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Extensions;
using LobotomyCorporationMods.Common.Implementations.Adapters;
using LobotomyCorporationMods.Common.Interfaces.Adapters;

namespace LobotomyCorporationMods.Common.Implementations.Facades
{
    public static class AbnormalityFacade
    {
        public static void ResetCrumblingArmorAgentList([NotNull] this ArmorCreature armorCreature,
            IArmorCreatureTestAdapter testAdapter = null)
        {
            Guard.Against.Null(armorCreature, nameof(armorCreature));

            testAdapter = testAdapter.EnsureNotNullWithMethod(() => new ArmorCreatureTestAdapter(armorCreature));

            testAdapter.SpecialAgentList.Clear();
            testAdapter.OnViewInit();
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
