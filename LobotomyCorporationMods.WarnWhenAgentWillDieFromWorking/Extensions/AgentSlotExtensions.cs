// SPDX-License-Identifier: MIT

#region

using CommandWindow;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Extensions;
using LobotomyCorporationMods.Common.Implementations;
using LobotomyCorporationMods.Common.Implementations.Adapters;
using LobotomyCorporationMods.Common.Interfaces.Adapters;

#endregion

namespace LobotomyCorporationMods.WarnWhenAgentWillDieFromWorking.Extensions
{
    public static class AgentSlotExtensions
    {
        public static bool CheckIfWorkWillKillAgent([NotNull] this AgentSlot agentSlot, [NotNull] CommandWindow.CommandWindow commandWindow)
        {
            var result = false;

            if (commandWindow.TryGetCreature(out var creature) && creature is not null)
            {
                result = CheckIfWorkWillKillAgent(agentSlot, commandWindow, new AnimationScriptAdapter(creature.GetAnimScript()));
            }

            return result;
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
