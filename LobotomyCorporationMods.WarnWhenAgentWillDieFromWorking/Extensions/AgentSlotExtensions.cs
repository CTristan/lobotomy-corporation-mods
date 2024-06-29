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
            IBeautyBeastAnimAdapter beautyBeastAnimAdapter,
            IYggdrasilAnimAdapter yggdrasilAnimAdapter)
        {
            var agent = agentSlot.CurrentAgent;
            var evaluator = commandWindow.GetCreatureEvaluator(agent, evaluators, beautyBeastAnimAdapter, yggdrasilAnimAdapter);

            return evaluator.WillAgentDie();
        }

        internal static void IndicateThatAgentWillDie([NotNull] this AgentSlot instance,
            [NotNull] IImageAdapter imageAdapter,
            [NotNull] ITextAdapter textAdapter)
        {
            var commandWindow = CommandWindow.CommandWindow.CurrentWindow;

            imageAdapter.GameObject = instance.WorkFilterFill;
            imageAdapter.Color = commandWindow.DeadColor;
            textAdapter.GameObject = instance.WorkFilterText;
            textAdapter.Text = LocalizeTextDataModel.instance.GetText("AgentState_Dead");
            instance.SetColor(commandWindow.DeadColor);
        }
    }
}
