// SPDX-License-Identifier: MIT

#region

using CommandWindow;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Interfaces.Adapters;

#endregion

namespace LobotomyCorporationMods.WarnWhenAgentWillDieFromWorking.Extensions
{
    internal static class AgentSlotExtensions
    {
        internal static bool CheckIfWorkWillKillAgent([NotNull] this AgentSlot agentSlot,
            CommandWindow.CommandWindow commandWindow, IBeautyBeastAnimAdapter beautyBeastAnimAdapter,
            IYggdrasilAnimAdapter yggdrasilAnimAdapter)
        {
            var agent = agentSlot.CurrentAgent;
            var evaluator = commandWindow.GetCreatureEvaluator(agent, beautyBeastAnimAdapter, yggdrasilAnimAdapter);

            return evaluator.WillAgentDie();
        }

        internal static void IndicateThatAgentWillDie([NotNull] this AgentSlot instance, [NotNull] IImageAdapter imageAdapter,
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
