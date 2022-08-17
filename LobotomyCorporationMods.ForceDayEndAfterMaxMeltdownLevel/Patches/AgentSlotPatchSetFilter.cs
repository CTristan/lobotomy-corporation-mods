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
        ///     Runs after the SetFilter method runs to check if we should disallow working with this room.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public static void Postfix([NotNull] AgentSlot __instance, AgentState state)
        {
            try
            {
                Guard.Against.Null(__instance, nameof(__instance));
                var commandWindow = CommandWindow.CommandWindow.CurrentWindow;
                var creatureOverloadManager = CreatureOverloadManager.instance;
                __instance.DisableIfMaxMeltdown(state, commandWindow, creatureOverloadManager);
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
