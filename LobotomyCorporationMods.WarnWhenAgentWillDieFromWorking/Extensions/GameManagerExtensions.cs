// SPDX-License-Identifier: MIT

using CommandWindow;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Implementations.Facades;

namespace LobotomyCorporationMods.WarnWhenAgentWillDieFromWorking.Extensions
{
    internal static class GameManagerExtensions
    {
        /// <summary>Checks if the current game stage is valid for the given AgentState.</summary>
        /// <param name="currentGameManager">The current GameManager instance.</param>
        /// <param name="state">The AgentState to check.</param>
        /// <returns>True if the game stage is valid, false otherwise.</returns>
        internal static bool IsValidGameStage([NotNull] this GameManager currentGameManager,
            AgentState state)
        {
            var commandWindow = CommandWindow.CommandWindow.CurrentWindow;

            return IsDayStarted(currentGameManager) && commandWindow.IsAbnormalityWorkWindow() && IsAgentControllable(state);
        }

        /// <summary>Checks if the day has already started.</summary>
        /// <param name="currentGameManager">The current GameManager instance.</param>
        /// <returns>True if the day has started, false otherwise.</returns>
        private static bool IsDayStarted([NotNull] GameManager currentGameManager)
        {
            return currentGameManager.ManageStarted;
        }

        /// <summary>Checks if the agent is controllable.</summary>
        /// <param name="state">The AgentState to check.</param>
        /// <returns>True if the agent is controllable, false otherwise.</returns>
        private static bool IsAgentControllable(AgentState state)
        {
            return !state.IsUncontrollable();
        }
    }
}
