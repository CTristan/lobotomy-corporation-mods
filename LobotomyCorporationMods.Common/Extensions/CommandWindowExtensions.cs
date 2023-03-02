// SPDX-License-Identifier: MIT

namespace LobotomyCorporationMods.Common.Extensions
{
    public static class CommandWindowExtensions
    {
        public static bool IsAbnormalityWorkWindow(this CommandWindow.CommandWindow commandWindow)
        {
            // Validation checks to confirm we have everything we need
            var isAbnormalityWorkWindow = commandWindow.CurrentSkill?.rwbpType is not null && commandWindow.CurrentWindowType == CommandType.Management;

            return isAbnormalityWorkWindow;
        }

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
