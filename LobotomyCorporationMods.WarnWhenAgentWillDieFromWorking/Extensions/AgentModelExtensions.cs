// SPDX-License-Identifier: MIT

using JetBrains.Annotations;

namespace LobotomyCorporationMods.WarnWhenAgentWillDieFromWorking.Extensions
{
    public static class AgentModelExtensions
    {
        public static bool CheckIfWorkWillKillAgent(this AgentModel agent, [NotNull] CommandWindow.CommandWindow commandWindow)
        {
            var evaluator = commandWindow.GetCreatureEvaluator(agent);

            return evaluator.WillAgentDie();
        }
    }
}
