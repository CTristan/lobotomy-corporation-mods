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
            if (!commandWindow.TryGetCreature(out var creature) || !creature.IsNotNull())
            {
                return null;
            }

            return creature.GetAbnormalityGift();
        }

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
