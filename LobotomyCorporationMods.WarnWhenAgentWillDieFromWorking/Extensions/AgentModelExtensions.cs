// SPDX-License-Identifier: MIT

using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Implementations;
using LobotomyCorporationMods.Common.Interfaces;

namespace LobotomyCorporationMods.WarnWhenAgentWillDieFromWorking.Extensions
{
    public static class AgentModelExtensions
    {
        public static bool CheckIfWorkWillKillAgent(this AgentModel agent, [NotNull] CommandWindow.CommandWindow commandWindow)
        {
            return CheckIfWorkWillKillAgent(agent, commandWindow, new AnimationScriptAdapter());
        }

        public static bool CheckIfWorkWillKillAgent(this AgentModel agent, [NotNull] CommandWindow.CommandWindow commandWindow, [NotNull] IAnimationScriptAdapter animationScriptAdapter)
        {
            var evaluator = commandWindow.GetCreatureEvaluator(agent, animationScriptAdapter);

            return evaluator.WillAgentDie();
        }
    }
}
