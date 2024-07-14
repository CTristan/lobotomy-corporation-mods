// SPDX-License-Identifier: MIT

using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Implementations;

namespace LobotomyCorporationMods.Common.Extensions
{
    internal static class CommandWindowExtensions
    {
        [CanBeNull]
        internal static CreatureEquipmentMakeInfo GetAbnormalityGift([NotNull] this CommandWindow.CommandWindow commandWindow)
        {
            return !commandWindow.TryGetCreature(out var creature) ? null : creature.GetAbnormalityGift();
        }

        [ContractAnnotation("=> true, creature:notnull; => false, creature:null")]
        private static bool TryGetCreature([NotNull] this CommandWindow.CommandWindow commandWindow,
            [CanBeNull] out CreatureModel creature)
        {
            Guard.Against.Null(commandWindow, nameof(commandWindow));

            creature = null;
            var unitModel = commandWindow.CurrentTarget;

            if (unitModel is CreatureModel creatureModel)
            {
                creature = creatureModel;
            }

            return creature.IsNotNull();
        }
    }
}
