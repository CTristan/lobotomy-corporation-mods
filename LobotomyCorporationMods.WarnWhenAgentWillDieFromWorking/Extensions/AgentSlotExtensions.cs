// SPDX-License-Identifier: MIT

#region

using System;
using System.Collections.Generic;
using CommandWindow;
using JetBrains.Annotations;
using LobotomyCorporation.Mods.Common.Enums;
using LobotomyCorporation.Mods.Common.Interfaces.Adapters;
using Hemocode.WarnWhenAgentWillDieFromWorking.Implementations;
using Hemocode.WarnWhenAgentWillDieFromWorking.Interfaces;

#endregion

namespace Hemocode.WarnWhenAgentWillDieFromWorking.Extensions
{
    public static class AgentSlotExtensions
    {
        internal static bool CheckIfWorkWillKillAgent([NotNull] this AgentSlot agentSlot,
            [NotNull] CommandWindow.CommandWindow commandWindow,
            Dictionary<CreatureIds, Func<CreatureEvaluatorParameters, ICreatureEvaluator>> evaluators,
            IBeautyBeastAnimTestAdapter beautyBeastAnimTestAdapter,
            IYggdrasilAnimTestAdapter yggdrasilAnimTestAdapter)
        {
            var agent = agentSlot.CurrentAgent;
            var evaluator = commandWindow.GetCreatureEvaluator(agent, evaluators, beautyBeastAnimTestAdapter, yggdrasilAnimTestAdapter);

            return evaluator.WillAgentDie();
        }
    }
}
