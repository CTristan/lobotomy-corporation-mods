// SPDX-License-Identifier: MIT

#region

using CommandWindow;
using LobotomyCorporationMods.Common.Interfaces.Adapters;

#endregion

namespace LobotomyCorporationMods.WarnWhenAgentWillDieFromWorking.Extensions
{
    internal static class AgentSlotExtensions
    {
        internal static bool CheckIfWorkWillKillAgent(this AgentSlot agentSlot, CommandWindow.CommandWindow commandWindow, IBeautyBeastAnimAdapter beautyBeastAnimAdapter,
            IGameObjectAdapter gameObjectAdapter)
        {
            bool willAgentDie;
            var agent = agentSlot.CurrentAgent;

            if (agent is not null)
            {
                var evaluator = commandWindow.GetCreatureEvaluator(agent, beautyBeastAnimAdapter, gameObjectAdapter);

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
