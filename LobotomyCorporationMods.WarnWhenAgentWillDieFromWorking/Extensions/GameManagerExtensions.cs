// SPDX-License-Identifier: MIT

using CommandWindow;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Extensions;

namespace LobotomyCorporationMods.WarnWhenAgentWillDieFromWorking.Extensions
{
    internal static class GameManagerExtensions
    {
        /// <summary>Checks if the current game stage is valid for the given AgentState.</summary>
        /// <param name="currentGameManager">The current GameManager instance.</param>
        /// <param name="state">The AgentState to check.</param>
        /// <returns>True if the game stage is valid, false otherwise.</returns>
        internal static bool IsValidGameStage([CanBeNull] this GameManager currentGameManager,
            AgentState state)
        {
            var commandWindow = CommandWindow.CommandWindow.CurrentWindow;
            return IsDayStarted(currentGameManager) && IsCorrectWindow(commandWindow) && IsAgentControllable(state);
        }

        /// <summary>Checks if the day has already started.</summary>
        /// <param name="currentGameManager">The current GameManager instance.</param>
        /// <returns>True if the day has started, false otherwise.</returns>
        private static bool IsDayStarted(GameManager currentGameManager)
        {
            return currentGameManager.IsNotNull() && currentGameManager.ManageStarted;
        }

        /// <summary>Checks if the current CommandWindow is an abnormality work window.</summary>
        /// <param name="commandWindow">The current CommandWindow to check.</param>
        /// <returns>True if the window is correct, false otherwise.</returns>
        private static bool IsCorrectWindow(CommandWindow.CommandWindow commandWindow)
        {
            return commandWindow.IsNotNull() && commandWindow.IsAbnormalityWorkWindow();
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
