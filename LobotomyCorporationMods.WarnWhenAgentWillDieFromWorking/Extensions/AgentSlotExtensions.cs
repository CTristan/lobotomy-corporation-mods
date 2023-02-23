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
            IYggdrasilAnimAdapter yggdrasilAnimAdapter)
        {
            var agent = agentSlot.CurrentAgent;
            var evaluator = commandWindow.GetCreatureEvaluator(agent, beautyBeastAnimAdapter, yggdrasilAnimAdapter);

            return evaluator.WillAgentDie();
        }
    }
}
