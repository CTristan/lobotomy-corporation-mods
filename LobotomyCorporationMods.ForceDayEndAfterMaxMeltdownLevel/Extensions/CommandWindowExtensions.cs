// SPDX-License-Identifier: MIT

using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Implementations;

// ReSharper disable once CheckNamespace
namespace CommandWindow
{
    internal static class CommandWindowExtensions
    {
        /// <summary>
        ///     Some initial Command Window checks to make sure we're in the right state.
        /// </summary>
        internal static bool IsValid([NotNull] this CommandWindow commandWindow)
        {
            Guard.Against.Null(commandWindow, nameof(commandWindow));

            // Validation checks to confirm we have everything we need
            if (commandWindow.CurrentSkill?.rwbpType == null)
            {
                return false;
            }

            return commandWindow.CurrentWindowType == CommandType.Management;
        }

        /// <summary>
        ///     Try to get a valid creature model from the Command Window.
        /// </summary>
        internal static bool TryGetCreatureModel([NotNull] this CommandWindow commandWindow, [CanBeNull] out CreatureModel creatureModel)
        {
            creatureModel = null;

            // Make sure we actually have an abnormality in our work window
            if (!(commandWindow.CurrentTarget is CreatureModel creature))
            {
                return false;
            }

            creatureModel = creature;

            // Make sure we have completed observation so we can't cheat
            return creatureModel.observeInfo.IsMaxObserved();
        }
    }
}
