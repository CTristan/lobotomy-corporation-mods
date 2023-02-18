// SPDX-License-Identifier: MIT

#region

using CommandWindow;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Extensions;
using LobotomyCorporationMods.Common.Implementations;
using LobotomyCorporationMods.Common.Interfaces.Adapters;

#endregion

namespace LobotomyCorporationMods.WarnWhenAgentWillDieFromWorking.Extensions
{
    internal static class AgentSlotExtensions
    {
        internal static bool CheckIfWorkWillKillAgent([NotNull] this AgentSlot agentSlot, [NotNull] CommandWindow.CommandWindow commandWindow, IBeautyBeastAnimAdapter beautyBeastAnimAdapter,
            IYggdrasilAnimAdapter yggdrasilAnimAdapter)
        {
            Guard.Against.Null(agentSlot, nameof(agentSlot));

            bool willAgentDie;
            var agent = agentSlot.CurrentAgent;

            if (agent is not null)
            {
                var evaluator = commandWindow.GetCreatureEvaluator(agent, beautyBeastAnimAdapter, yggdrasilAnimAdapter);

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
