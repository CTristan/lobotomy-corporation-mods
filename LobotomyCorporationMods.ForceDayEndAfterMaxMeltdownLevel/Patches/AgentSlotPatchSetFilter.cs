// SPDX-License-Identifier: MIT

using System;
using System.Diagnostics.CodeAnalysis;
using CommandWindow;
using Harmony;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Implementations;

namespace LobotomyCorporationMods.ForceDayEndAfterMaxMeltdownLevel.Patches
{
    [HarmonyPatch(typeof(AgentSlot), "SetFilter", new[] { typeof(AgentState) })]
    [SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores")]
    public static class AgentSlotPatchSetFilter
    {
        /// <summary>
        ///     Runs before the SetFilter method runs to check if we should disable the agent slot.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public static bool Prefix([NotNull] AgentSlot __instance, AgentState state)
        {
            try
            {
                Guard.Against.Null(__instance, nameof(__instance));

                var commandWindow = CommandWindow.CommandWindow.CurrentWindow;
                var creatureOverloadManager = CreatureOverloadManager.instance;
                var playerModel = PlayerModel.instance;
                var stageTypeInfo = StageTypeInfo.instnace;
                var energyModel = EnergyModel.instance;

                if (state != AgentState.UNCONTROLLABLE || !__instance.IsMaxMeltdown(state, commandWindow, creatureOverloadManager, playerModel, stageTypeInfo, energyModel))
                {
                    return true;
                }

                __instance.FilterControl.SetActive(true);
                __instance.FilterFill.color = CommandWindow.CommandWindow.CurrentWindow.UnconColor;
                __instance.FilterText.text = LocalizeTextDataModel.instance.GetText("AgentState_EndOfDay");
                __instance.SetColor(CommandWindow.CommandWindow.CurrentWindow.UnconColor);

                return false;
            }
            catch (Exception ex)
            {
                // Null argument exception only comes up during testing due to Unity operator overloading.
                // https://github.com/JetBrains/resharper-unity/wiki/Possible-unintended-bypass-of-lifetime-check-of-underlying-Unity-engine-object
                if (ex is ArgumentNullException)
                {
                    return true;
                }

                Harmony_Patch.Instance.FileManager.WriteToLog(ex);

                throw;
            }
        }

        /// <summary>
        ///     Runs after the SetFilter method runs to check if we should set the state as unworkable.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public static void Postfix([NotNull] AgentSlot __instance, AgentState state)
        {
            try
            {
                Guard.Against.Null(__instance, nameof(__instance));

                var commandWindow = CommandWindow.CommandWindow.CurrentWindow;
                var creatureOverloadManager = CreatureOverloadManager.instance;
                var playerModel = PlayerModel.instance;
                var stageTypeInfo = StageTypeInfo.instnace;
                var energyModel = EnergyModel.instance;

                if (__instance.IsMaxMeltdown(state, commandWindow, creatureOverloadManager, playerModel, stageTypeInfo, energyModel))
                {
                    __instance.State = AgentState.UNCONTROLLABLE;
                }
            }
            catch (Exception ex)
            {
                // Null argument exception only comes up during testing due to Unity operator overloading.
                // https://github.com/JetBrains/resharper-unity/wiki/Possible-unintended-bypass-of-lifetime-check-of-underlying-Unity-engine-object
                if (ex is ArgumentNullException)
                {
                    return;
                }

                Harmony_Patch.Instance.FileManager.WriteToLog(ex);

                throw;
            }
        }
    }
}
