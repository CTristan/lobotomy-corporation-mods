// SPDX-License-Identifier: MIT

#region

using System;
using System.Diagnostics.CodeAnalysis;
using Harmony;
using JetBrains.Annotations;
using Hemocode.BadLuckProtectionForGifts.Interfaces;
using LobotomyCorporation.Mods.Common.Attributes;
using LobotomyCorporation.Mods.Common.Constants;
using LobotomyCorporation.Mods.Common.Implementations;

#endregion

namespace Hemocode.BadLuckProtectionForGifts.Patches
{
    [HarmonyPatch(typeof(GameSceneController), nameof(GameSceneController.OnStageStart))]
    public static class GameSceneControllerPatchOnStageStart
    {
        public static void PatchAfterOnStageStart([NotNull] IAgentWorkTracker agentWorkTracker)
        {
            ThrowHelper.ThrowIfNull(agentWorkTracker, nameof(agentWorkTracker));

            agentWorkTracker.Load();
        }

        public static void PostfixWithLogging(Func<IAgentWorkTracker> getAgentWorkTracker)
        {
            try
            {
                ThrowHelper.ThrowIfNull(getAgentWorkTracker, nameof(getAgentWorkTracker));

                PatchAfterOnStageStart(getAgentWorkTracker());
            }
            catch (Exception ex)
            {
                Harmony_Patch.Instance.Logger.WriteException(ex);

                throw;
            }
        }

        /// <summary>
        ///     Runs after the original OnStageStart method to reset our tracker progress. We reset the progress on restart because it doesn't make sense that an agent would remember
        ///     their creature experience if the day is reset.
        /// </summary>
        [EntryPoint]
        [ExcludeFromCodeCoverage(Justification = Messages.UnityCodeCoverageJustification)]
        public static void Postfix()
        {
            PostfixWithLogging(() => Harmony_Patch.Instance.AgentWorkTracker);
        }
    }
}
