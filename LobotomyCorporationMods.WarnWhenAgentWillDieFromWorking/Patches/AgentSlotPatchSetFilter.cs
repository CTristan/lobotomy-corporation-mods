// SPDX-License-Identifier: MIT

#region

using System;
using System.Diagnostics.CodeAnalysis;
using CommandWindow;
using Harmony;
using LobotomyCorporationMods.Common.Attributes;
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
                __instance.PatchAfterSetFilter(state, new BeautyBeastAnimAdapter(), new ImageAdapter(), new TextAdapter(), new YggdrasilAnimAdapter());
            }
            catch (Exception ex)
            {
                Harmony_Patch.Instance.Logger.WriteToLog(ex);

                throw;
            }
        }

        public static void PatchAfterSetFilter(this AgentSlot __instance, AgentState state, IBeautyBeastAnimAdapter beautyBeastAnimAdapter, IImageAdapter imageAdapter, ITextAdapter textAdapter,
            IYggdrasilAnimAdapter yggdrasilAnimAdapter)
        {
            if (__instance is null)
            {
                throw new ArgumentNullException(nameof(__instance));
            }

            if (beautyBeastAnimAdapter is null)
            {
                throw new ArgumentNullException(nameof(beautyBeastAnimAdapter));
            }

            if (imageAdapter is null)
            {
                throw new ArgumentNullException(nameof(imageAdapter));
            }

            if (textAdapter is null)
            {
                throw new ArgumentNullException(nameof(textAdapter));
            }

            if (yggdrasilAnimAdapter is null)
            {
                throw new ArgumentNullException(nameof(yggdrasilAnimAdapter));
            }

            // Some initial Command Window checks to make sure we're in the right state
            var commandWindow = CommandWindow.CommandWindow.CurrentWindow;
            if (commandWindow is not null && commandWindow.IsAbnormalityWorkWindow() && !state.IsUncontrollable())
            {
                var agentWillDie = __instance.CheckIfWorkWillKillAgent(commandWindow, beautyBeastAnimAdapter, yggdrasilAnimAdapter);

                if (agentWillDie)
                {
                    imageAdapter.GameObject = __instance.WorkFilterFill;
                    imageAdapter.Color = commandWindow.DeadColor;

                    textAdapter.GameObject = __instance.WorkFilterText;
                    textAdapter.Text = LocalizeTextDataModel.instance.GetText("AgentState_Dead");

                    __instance.SetColor(commandWindow.DeadColor);
                }
            }
        }
    }
}
