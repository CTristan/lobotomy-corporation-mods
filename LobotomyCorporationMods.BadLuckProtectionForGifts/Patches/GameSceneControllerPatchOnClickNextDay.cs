// SPDX-License-Identifier: MIT

#region

using System;
using System.Diagnostics.CodeAnalysis;
using Harmony;
using JetBrains.Annotations;
using Hemocode.BadLuckProtectionForGifts.Interfaces;
using Hemocode.Common.Attributes;
using Hemocode.Common.Constants;
using Hemocode.Common.Implementations;

#endregion

namespace Hemocode.BadLuckProtectionForGifts.Patches
{
    [HarmonyPatch(typeof(GameSceneController), nameof(GameSceneController.OnClickNextDay))]
    public static class GameSceneControllerPatchOnClickNextDay
    {
        public static void PatchAfterOnClickNextDay([NotNull] IAgentWorkTracker agentWorkTracker)
        {
            ThrowHelper.ThrowIfNull(agentWorkTracker, nameof(agentWorkTracker));

            agentWorkTracker.Save();
        }

        public static void PostfixWithLogging(Func<IAgentWorkTracker> getAgentWorkTracker)
        {
            try
            {
                ThrowHelper.ThrowIfNull(getAgentWorkTracker, nameof(getAgentWorkTracker));

                PatchAfterOnClickNextDay(getAgentWorkTracker());
            }
            catch (Exception ex)
            {
                Harmony_Patch.Instance.Logger.WriteException(ex);

                throw;
            }
        }

        /// <summary>
        ///     Runs after the original OnClickNextDay method to save our tracker progress. We only save when going to the next day because it doesn't make sense that an agent would
        ///     remember their creature experience if the day is reset.
        /// </summary>
        [EntryPoint]
        [ExcludeFromCodeCoverage(Justification = Messages.UnityCodeCoverageJustification)]
        public static void Postfix()
        {
            PostfixWithLogging(() => Harmony_Patch.Instance.AgentWorkTracker);
        }
    }
}
