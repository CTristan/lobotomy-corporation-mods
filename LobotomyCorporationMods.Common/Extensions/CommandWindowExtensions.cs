// SPDX-License-Identifier: MIT

namespace LobotomyCorporationMods.Common.Extensions
{
    public static class CommandWindowExtensions
    {
        public static bool TryGetCreature(this CommandWindow.CommandWindow commandWindow, out CreatureModel? creature)
        {
            creature = null;

            var unitModel = commandWindow.CurrentTarget;
            if (unitModel is CreatureModel creatureModel)
            {
                creature = creatureModel;
            }

            return creature is not null;
        }
    }
}
