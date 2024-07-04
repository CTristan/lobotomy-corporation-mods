// SPDX-License-Identifier: MIT

using System.Linq;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Extensions;
using LobotomyCorporationMods.Common.Implementations.Adapters;
using LobotomyCorporationMods.Common.Interfaces.Adapters;

namespace LobotomyCorporationMods.Common.Implementations.Facades
{
    public static class AbnormalityFacade
    {
        public static int GetParasiteTreeNumberOfFlowers(this CreatureModel creatureModel,
            [CanBeNull] IYggdrasilAnimTestAdapter yggdrasilAnimTestAdapter = null)
        {
            yggdrasilAnimTestAdapter = yggdrasilAnimTestAdapter.EnsureNotNullWithMethod(() => new YggdrasilAnimTestAdapter((YggdrasilAnim)creatureModel.GetAnimScript()));

            return yggdrasilAnimTestAdapter.Flowers.Count(flower => flower.ActiveSelf);
        }

        public static bool IsBeautyAndTheBeastWeakened(this CreatureModel creatureModel,
            [CanBeNull] IBeautyBeastAnimTestAdapter beautyBeastAnimTestAdapter = null)
        {
            const int WeakenedState = 1;

            beautyBeastAnimTestAdapter = beautyBeastAnimTestAdapter.EnsureNotNullWithMethod(() => new BeautyBeastAnimTestAdapter((BeautyBeastAnim)creatureModel.GetAnimScript()));
            var animationState = beautyBeastAnimTestAdapter.State;

            return animationState == WeakenedState;
        }

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
