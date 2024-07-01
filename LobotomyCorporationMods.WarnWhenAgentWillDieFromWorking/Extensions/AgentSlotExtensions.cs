// SPDX-License-Identifier: MIT

#region

using System;
using System.Collections.Generic;
using CommandWindow;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Enums;
using LobotomyCorporationMods.Common.Interfaces.Adapters;
using LobotomyCorporationMods.WarnWhenAgentWillDieFromWorking.Implementations;
using LobotomyCorporationMods.WarnWhenAgentWillDieFromWorking.Interfaces;

#endregion

namespace LobotomyCorporationMods.WarnWhenAgentWillDieFromWorking.Extensions
{
    internal static class AgentSlotExtensions
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

        internal static void IndicateThatAgentWillDie([NotNull] this AgentSlot instance,
            [NotNull] IImageTestAdapter imageTestAdapter,
            [NotNull] ITextTestAdapter textTestAdapter)
        {
            var commandWindow = CommandWindow.CommandWindow.CurrentWindow;

            imageTestAdapter.GameObject = instance.WorkFilterFill;
            imageTestAdapter.Color = commandWindow.DeadColor;
            textTestAdapter.GameObject = instance.WorkFilterText;
            textTestAdapter.Text = LocalizeTextDataModel.instance.GetText("AgentState_Dead");
            instance.SetColor(commandWindow.DeadColor);
        }
    }
}
