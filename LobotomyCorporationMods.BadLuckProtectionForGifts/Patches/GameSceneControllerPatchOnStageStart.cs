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
    [HarmonyPatch(typeof(GameSceneController), "OnStageStart")]
    public static class GameSceneControllerPatchOnStageStart
    {
        public static void PatchAfterOnStageStart(IAgentWorkTracker agentWorkTracker)
        {
            Guard.Against.Null(agentWorkTracker, nameof(agentWorkTracker));

            agentWorkTracker.Load();
        }

        /// <summary>
        ///     Runs after the original OnStageStart method to reset our tracker progress. We reset the progress on restart
        ///     because it doesn't make sense that an agent would remember their creature experience if the day is reset.
        /// </summary>
        [EntryPoint]
        [ExcludeFromCodeCoverage]
        public static void Postfix()
        {
            try
            {
                PatchAfterOnStageStart(Harmony_Patch.Instance.AgentWorkTracker);
            }
            catch (Exception ex)
            {
                Harmony_Patch.Instance.Logger.WriteToLog(ex);

                throw;
            }
        }
    }
}
