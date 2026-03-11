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

        /// <summary>
        ///     Forces Crumbling Armor's internal list of agents to be re-initialized. This fixes issues where Crumbling Armor's internal list of agents wasn't updated properly and
        ///     either has agents it shouldn't or is missing agents it should have.
        /// </summary>
        /// <param name="armorCreature">The instance of Crumbling Armor.</param>
        /// <param name="testAdapter">Optional test adapter to use. If not provided, a new instance will be created.</param>
        public static void ReloadCrumblingArmorAgentList([NotNull] this ArmorCreature armorCreature,
            IArmorCreatureTestAdapter testAdapter = null)
        {
            ThrowHelper.ThrowIfNull(armorCreature);

            testAdapter = testAdapter.EnsureNotNullWithMethod(() => new ArmorCreatureTestAdapter(armorCreature));

            testAdapter.ReloadSpecialAgentList();
        }
    }
}
