// SPDX-License-Identifier: MIT

using JetBrains.Annotations;

namespace LobotomyCorporationMods.WarnWhenAgentWillDieFromWorking.Extensions
{
    internal static class CommandWindowExtensions
    {
        [CanBeNull]
        internal static CreatureModel GetCreatureIfValid([NotNull] this CommandWindow.CommandWindow commandWindow)
        {
            if (!(commandWindow.CurrentTarget is CreatureModel creature))
            {
                return null;
            }

            // Make sure we have completed observation so we can't cheat
            return !creature.observeInfo.IsMaxObserved() ? null : creature;
        }
    }
}
