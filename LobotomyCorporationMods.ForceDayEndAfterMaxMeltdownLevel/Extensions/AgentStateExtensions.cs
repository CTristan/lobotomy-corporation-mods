// SPDX-License-Identifier: MIT

// ReSharper disable once CheckNamespace

namespace CommandWindow
{
    internal static class AgentStateExtensions
    {
        internal static bool IsUncontrollable(this AgentState state)
        {
            return state == AgentState.DEAD || state == AgentState.PANIC || state == AgentState.UNCONTROLLABLE;
        }
    }
}
