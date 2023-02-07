// SPDX-License-Identifier: MIT

using System;
using System.Diagnostics.CodeAnalysis;
using CommandWindow;
using Harmony;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Extensions;
using LobotomyCorporationMods.Common.Implementations;
using LobotomyCorporationMods.WarnWhenAgentWillDieFromWorking.Extensions;

namespace LobotomyCorporationMods.WarnWhenAgentWillDieFromWorking.Patches
{
    [HarmonyPatch(typeof(AgentSlot), "SetFilter")]
    public static class AgentSlotPatchSetFilter
    {
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
                if (!(commandWindow is null) && commandWindow.IsAbnormalityWorkWindow() && !state.IsUncontrollable())
                {
                    var agentWillDie = __instance.CheckIfWorkWillKillAgent(commandWindow);

                    if (agentWillDie)
                    {
                        __instance.WorkFilterFill.color = commandWindow.DeadColor;
                        __instance.WorkFilterText.text = LocalizeTextDataModel.instance.GetText("AgentState_Dead");
                        __instance.SetColor(commandWindow.DeadColor);
                    }
                }
            }
            catch (Exception ex)
            {
                Harmony_Patch.Instance.FileManager.WriteToLog(ex);

                throw;
            }
        }
    }
}
