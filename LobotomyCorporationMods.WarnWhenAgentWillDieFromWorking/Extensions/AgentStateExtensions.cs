// SPDX-License-Identifier: MIT

#region

using CommandWindow;

#endregion

namespace LobotomyCorporationMods.WarnWhenAgentWillDieFromWorking.Extensions
{
    internal static class AgentStateExtensions
    {
        internal static bool IsUncontrollable(this AgentState state)
        {
            return state == AgentState.DEAD || state == AgentState.PANIC || state == AgentState.UNCONTROLLABLE;
        }
    }
}
