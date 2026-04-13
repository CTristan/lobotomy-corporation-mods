// SPDX-License-Identifier: MIT

#region

using System;
using System.Diagnostics.CodeAnalysis;
using Harmony;
using JetBrains.Annotations;
using LobotomyCorporation.Mods.Common;
using LobotomyCorporationMods.BadLuckProtectionForGifts.Interfaces;

#endregion

namespace LobotomyCorporationMods.BadLuckProtectionForGifts.Patches
{
    [HarmonyPatch(typeof(GameSceneController), nameof(GameSceneController.OnStageStart))]
    public static class GameSceneControllerPatchOnStageStart
    {
        public static void PatchAfterOnStageStart([NotNull] IAgentWorkTracker agentWorkTracker)
        {
            ThrowHelper.ThrowIfNull(agentWorkTracker, nameof(agentWorkTracker));

            agentWorkTracker.Load();
        }

        /// <summary>
        ///     Runs after the original OnStageStart method to reset our tracker progress. We reset the progress on restart because it doesn't make sense that an agent would remember
        ///     their creature experience if the day is reset.
        /// </summary>
        [EntryPoint]
        [ExcludeFromCodeCoverage(Justification = Messages.UnityCodeCoverageJustification)]
        public static void Postfix()
        {
            try
            {
                PatchAfterOnStageStart(Harmony_Patch.Instance.AgentWorkTracker);
            }
            catch (Exception ex)
            {
                Harmony_Patch.Instance.Logger.WriteException(ex);

                throw;
            }
        }
    }
}
