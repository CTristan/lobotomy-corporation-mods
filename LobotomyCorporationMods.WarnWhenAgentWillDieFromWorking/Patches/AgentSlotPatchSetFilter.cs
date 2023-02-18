// SPDX-License-Identifier: MIT

#region

using System;
using System.Diagnostics.CodeAnalysis;
using CommandWindow;
using Harmony;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Extensions;
using LobotomyCorporationMods.Common.Implementations;
using LobotomyCorporationMods.Common.Implementations.Adapters;
using LobotomyCorporationMods.Common.Interfaces.Adapters;
using LobotomyCorporationMods.WarnWhenAgentWillDieFromWorking.Extensions;

#endregion

namespace LobotomyCorporationMods.WarnWhenAgentWillDieFromWorking.Patches
{
    [HarmonyPatch(typeof(AgentSlot), "SetFilter")]
    public static class AgentSlotPatchSetFilter
    {
        public static IYggdrasilAnimAdapter AnimAdapter { private get; set; }
        public static IBeautyBeastAnimAdapter BeastAnimAdapter { private get; set; }
        public static IImageAdapter WorkFilterFillAdapter { private get; set; }
        public static ITextAdapter WorkFilterTextAdapter { private get; set; }

        [SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores")]
        [SuppressMessage("Style", "IDE1006:Naming Styles")]
        // ReSharper disable once InconsistentNaming
        public static void Postfix([NotNull] AgentSlot __instance, AgentState state)
        {
            try
            {
                Guard.Against.Null(__instance, nameof(__instance));
                Guard.Against.Null(state, nameof(state));

                // Some initial Command Window checks to make sure we're in the right state
                var commandWindow = CommandWindow.CommandWindow.CurrentWindow;
                if (commandWindow is not null && commandWindow.IsAbnormalityWorkWindow() && !state.IsUncontrollable())
                {
                    var agentWillDie = __instance.CheckIfWorkWillKillAgent(commandWindow, BeastAnimAdapter, AnimAdapter);

                    if (agentWillDie)
                    {
                        WorkFilterFillAdapter ??= new ImageAdapter(__instance.WorkFilterFill);
                        WorkFilterFillAdapter.Color = commandWindow.DeadColor;

                        WorkFilterTextAdapter ??= new TextAdapter(__instance.WorkFilterText);
                        WorkFilterTextAdapter.Text = LocalizeTextDataModel.instance.GetText("AgentState_Dead");

                        __instance.SetColor(commandWindow.DeadColor);
                    }
                }
            }
            catch (Exception ex)
            {
                Harmony_Patch.Instance.Logger.WriteToLog(ex);

                throw;
            }
        }
    }
}
