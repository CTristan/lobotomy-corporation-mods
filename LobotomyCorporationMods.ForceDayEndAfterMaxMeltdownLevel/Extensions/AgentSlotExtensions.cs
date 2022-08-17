// SPDX-License-Identifier: MIT

using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Implementations;

// ReSharper disable once CheckNamespace
namespace CommandWindow
{
    public static class AgentSlotExtensions
    {
        public static void DisableIfMaxMeltdown([NotNull] this AgentSlot agentSlot, AgentState state, [NotNull] CommandWindow commandWindow, CreatureOverloadManager creatureOverloadManager)
        {
            Guard.Against.Null(agentSlot, nameof(agentSlot));
            Guard.Against.Null(commandWindow, nameof(commandWindow));

            if (!TryGetCreatureModel(commandWindow, out var creatureModel) || creatureModel == null)
            {
                return;
            }

            if (IsInvalidState(state) || !IsValidCommandWindow(commandWindow) || !IsMaxMeltdown(creatureOverloadManager) || !IsRoomInMeltdown(creatureModel))
            {
                return;
            }

            // Now that we're past all of our checks, disallow working on this slot
            agentSlot.SetColor(commandWindow.UnconColor);
            agentSlot.State = AgentState.UNCONTROLLABLE;
        }

        /// <summary>
        ///     Try to get a valid creature model from the Command Window.
        /// </summary>
        private static bool TryGetCreatureModel([NotNull] CommandWindow commandWindow, [CanBeNull] out CreatureModel creatureModel)
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

        private static bool IsInvalidState(AgentState state)
        {
            return state == AgentState.DEAD || state == AgentState.PANIC || state == AgentState.UNCONTROLLABLE;
        }

        /// <summary>
        ///     Some initial Command Window checks to make sure we're in the right state.
        /// </summary>
        private static bool IsValidCommandWindow([NotNull] CommandWindow commandWindow)
        {
            Guard.Against.Null(commandWindow, nameof(commandWindow));

            // Validation checks to confirm we have everything we need
            if (commandWindow.CurrentSkill?.rwbpType == null)
            {
                return false;
            }

            return commandWindow.CurrentWindowType == CommandType.Management;
        }

        private static bool IsMaxMeltdown([NotNull] CreatureOverloadManager creatureOverloadManager)
        {
            const int MaxMeltdownLevel = 10;
            var meltdownLevel = creatureOverloadManager.GetQliphothOverloadLevel();

            return meltdownLevel >= MaxMeltdownLevel;
        }

        private static bool IsRoomInMeltdown([NotNull] CreatureModel creature)
        {
            // If the abnormality is under meltdown, allow working so the player can take care of it
            var room = creature.Unit.room;

            return !room.overloadUI.isActivated;
        }
    }
}
