// SPDX-License-Identifier: MIT

#region

using System;
using System.Diagnostics.CodeAnalysis;
using CommandWindow;
using Harmony;
using LobotomyCorporationMods.Common.Implementations.Adapters;
using LobotomyCorporationMods.Common.Interfaces.Adapters;
using LobotomyCorporationMods.WarnWhenAgentWillDieFromWorking.Extensions;

#endregion

namespace LobotomyCorporationMods.WarnWhenAgentWillDieFromWorking.Patches
{
    [HarmonyPatch(typeof(AgentSlot), "SetFilter")]
    public static class AgentSlotPatchSetFilter
    {
        public static IYggdrasilAnimAdapter? AnimAdapter { get; set; }
        public static IBeautyBeastAnimAdapter? BeastAnimAdapter { get; set; }
        public static IImageAdapter? WorkFilterFillAdapter { get; set; }
        public static ITextAdapter? WorkFilterTextAdapter { get; set; }

        [SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores")]
        [SuppressMessage("Style", "IDE1006:Naming Styles")]
        // ReSharper disable once InconsistentNaming
        public static void Postfix(AgentSlot? __instance, AgentState state)
        {
            try
            {
                if (__instance is null)
                {
                    throw new ArgumentNullException(nameof(__instance));
                }

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
