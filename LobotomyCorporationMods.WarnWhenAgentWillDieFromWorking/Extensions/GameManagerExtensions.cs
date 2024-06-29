// SPDX-License-Identifier: MIT

using CommandWindow;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Extensions;

namespace LobotomyCorporationMods.WarnWhenAgentWillDieFromWorking.Extensions
{
    internal static class GameManagerExtensions
    {
        internal static bool IsValidGameStage([CanBeNull] this GameManager currentGameManager,
            AgentState state)
        {
            // First load won't have a game manager yet, so just gracefully exit
            if (currentGameManager.IsNull())
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

            return !commandWindow.IsNull() && commandWindow.IsAbnormalityWorkWindow() && !state.IsUncontrollable();
        }
    }
}
