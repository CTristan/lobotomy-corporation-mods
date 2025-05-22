// SPDX-License-Identifier: MIT

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace LobotomyCorporationMods.Common.Extensions
{
    public static class AgentManagerExtensions
    {
        [NotNull]
        internal static List<AgentModel> GetControllableAgents(this AgentManager agentManager)
        {
            return agentManager.GetAgentList().Where(a => a.IsControllable()).ToList();
        }

        [NotNull]
        internal static List<AgentModel> GetLivingAgents(this AgentManager agentManager)
        {
            return agentManager.GetAgentList().Where(a => !a.IsDead()).ToList();
        }
    }
}
