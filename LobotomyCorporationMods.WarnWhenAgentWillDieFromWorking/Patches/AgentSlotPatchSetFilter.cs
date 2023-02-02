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
    public class AgentSlotPatchSetFilter
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

                if (state.IsInvalidState()) { return; }

                // Some initial Command Window checks to make sure we're in the right state
                var commandWindow = GetCommandWindowIfValid();
                if (commandWindow == null) { return; }

                var agentWillDie = __instance.CurrentAgent.CheckIfWorkWillKillAgent(commandWindow);

                if (!agentWillDie)
                {
                    return;
                }

                __instance.WorkFilterFill.color = commandWindow.DeadColor;
                __instance.WorkFilterText.text = LocalizeTextDataModel.instance.GetText("AgentState_Dead");
                __instance.SetColor(commandWindow.DeadColor);
            }
            catch (Exception ex)
            {
                Harmony_Patch.Instance.FileManager.WriteToLog(ex);

                throw;
            }
        }

        [CanBeNull]
        private static CommandWindow.CommandWindow GetCommandWindowIfValid()
        {
            var commandWindow = CommandWindow.CommandWindow.CurrentWindow;
            if (commandWindow == null) { return null; }

            // Validation checks to confirm we have everything we need
            if (commandWindow.CurrentSkill?.rwbpType == null) { return null; }

            return commandWindow.CurrentWindowType != CommandType.Management ? null : commandWindow;
        }
    }
}
