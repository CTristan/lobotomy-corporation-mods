// SPDX-License-Identifier: MIT

#region

using System;
using System.Diagnostics.CodeAnalysis;
using Harmony;
using LobotomyCorporationMods.BadLuckProtectionForGifts.Interfaces;
using LobotomyCorporationMods.Common.Attributes;
using LobotomyCorporationMods.Common.Extensions;
using LobotomyCorporationMods.Common.Implementations;

#endregion

namespace LobotomyCorporationMods.BadLuckProtectionForGifts.Patches
{
    [HarmonyPatch(typeof(GameSceneController), "OnClickNextDay")]
    public static class GameSceneControllerPatchOnClickNextDay
    {
        public static void PatchAfterOnClickNextDay(IAgentWorkTracker agentWorkTracker)
        {
            Guard.Against.Null(agentWorkTracker, nameof(agentWorkTracker));

            agentWorkTracker.Save();
        }

        /// <summary>
        ///     Runs after the original OnClickNextDay method to save our tracker progress. We only save when going to the next
        ///     day because it doesn't make sense that an agent would remember their creature experience if the day is reset.
        /// </summary>
        [EntryPoint]
        [ExcludeFromCodeCoverage]
        public static void Postfix()
        {
            try
            {
                PatchAfterOnClickNextDay(Harmony_Patch.Instance.AgentWorkTracker);
            }
            catch (Exception ex)
            {
                Harmony_Patch.Instance.Logger.WriteToLog(ex);

                throw;
            }
        }
    }
}
