// SPDX-License-Identifier: MIT

#region

using System;
using System.Diagnostics.CodeAnalysis;
using CommandWindow;
using Harmony;
using LobotomyCorporationMods.Common.Attributes;
using LobotomyCorporationMods.Common.Extensions;
using LobotomyCorporationMods.Common.Implementations.Adapters;
using LobotomyCorporationMods.Common.Interfaces.Adapters;
using LobotomyCorporationMods.WarnWhenAgentWillDieFromWorking.Extensions;

#endregion

namespace LobotomyCorporationMods.WarnWhenAgentWillDieFromWorking.Patches
{
    [HarmonyPatch(typeof(AgentSlot), "SetFilter")]
    public static class AgentSlotPatchSetFilter
    {
        // ReSharper disable InconsistentNaming
        [EntryPoint]
        [ExcludeFromCodeCoverage]
        public static void Postfix(AgentSlot __instance, AgentState state)
        {
            try
            {
                var currentGameManager = GameManager.currentGameManager;
                __instance.PatchAfterSetFilter(state, currentGameManager, new BeautyBeastAnimAdapter(), new ImageAdapter(), new TextAdapter(), new YggdrasilAnimAdapter());
            }
            catch (Exception ex)
            {
                Harmony_Patch.Instance.Logger.WriteToLog(ex);

                throw;
            }
        }

        public static void PatchAfterSetFilter(this AgentSlot instance, AgentState state, GameManager? currentGameManager, IBeautyBeastAnimAdapter beautyBeastAnimAdapter, IImageAdapter imageAdapter,
            ITextAdapter textAdapter, IYggdrasilAnimAdapter yggdrasilAnimAdapter)
        {
            if (instance is null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            // First load won't have a game manager yet, so just gracefully exit
            if (currentGameManager is null)
            {
                return;
            }

            // If we're not in Management phase then we don't need to check anything
            if (!currentGameManager.ManageStarted)
            {
                return;
            }

            // Some initial Command Window checks to make sure we're in the right state
            var commandWindow = CommandWindow.CommandWindow.CurrentWindow;
            if (commandWindow is not null && commandWindow.IsAbnormalityWorkWindow() && !state.IsUncontrollable())
            {
                var agentWillDie = instance.CheckIfWorkWillKillAgent(commandWindow, beautyBeastAnimAdapter, yggdrasilAnimAdapter);

                if (agentWillDie)
                {
                    imageAdapter.GameObject = instance.WorkFilterFill;
                    imageAdapter.Color = commandWindow.DeadColor;

                    textAdapter.GameObject = instance.WorkFilterText;
                    textAdapter.Text = LocalizeTextDataModel.instance.GetText("AgentState_Dead");

                    instance.SetColor(commandWindow.DeadColor);
                }
            }
        }
    }
}
