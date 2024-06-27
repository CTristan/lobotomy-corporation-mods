// SPDX-License-Identifier: MIT

using CommandWindow;
using JetBrains.Annotations;

namespace LobotomyCorporationMods.WarnWhenAgentWillDieFromWorking.Extensions
{
    internal static class GameManagerExtensions
    {
        internal static bool IsValidGameStage([CanBeNull] this GameManager currentGameManager, AgentState state)
        {
            // First load won't have a game manager yet, so just gracefully exit
            if (currentGameManager is null)
            {
                return false;
            }

            // If we're not in Management phase then we don't need to check anything
            if (!currentGameManager.ManageStarted)
            {
                return false;
            }

            // Some initial Command Window checks to make sure we're in the right state
            var commandWindow = CommandWindow.CommandWindow.CurrentWindow;
            if (commandWindow is null || !commandWindow.IsAbnormalityWorkWindow() || state.IsUncontrollable())
            {
                return false;
            }

            return true;
        }
    }
}
