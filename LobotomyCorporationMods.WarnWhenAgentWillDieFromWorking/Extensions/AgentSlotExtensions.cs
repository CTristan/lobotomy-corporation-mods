// SPDX-License-Identifier: MIT

using CommandWindow;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Extensions;
using LobotomyCorporationMods.Common.Implementations;
using LobotomyCorporationMods.Common.Implementations.Adapters;
using LobotomyCorporationMods.Common.Interfaces.Adapters;

namespace LobotomyCorporationMods.WarnWhenAgentWillDieFromWorking.Extensions
{
    public static class AgentSlotExtensions
    {
        public static bool CheckIfWorkWillKillAgent([NotNull] this AgentSlot agentSlot, [NotNull] CommandWindow.CommandWindow commandWindow)
        {
            return CheckIfWorkWillKillAgent(agentSlot, commandWindow, new AnimationScriptAdapter());
        }

        public static bool CheckIfWorkWillKillAgent([NotNull] this AgentSlot agentSlot, [NotNull] CommandWindow.CommandWindow commandWindow, [NotNull] IAnimationScriptAdapter animationScriptAdapter)
        {
            Guard.Against.Null(agentSlot, nameof(agentSlot));

            bool willAgentDie;
            var agent = agentSlot.CurrentAgent;

            if (agent is not null)
            {
                var evaluator = commandWindow.GetCreatureEvaluator(agent, animationScriptAdapter);

                willAgentDie = evaluator.WillAgentDie();
            }
            else
            {
                willAgentDie = false;
            }

            return willAgentDie;
        }
    }
}
