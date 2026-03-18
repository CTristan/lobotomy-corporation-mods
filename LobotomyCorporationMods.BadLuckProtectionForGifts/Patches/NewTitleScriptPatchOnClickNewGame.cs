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
    [HarmonyPatch(typeof(NewTitleScript), nameof(NewTitleScript.OnClickNewGame))]
    public static class NewTitleScriptPatchOnClickNewGame
    {
        public static void PatchAfterOnClickNewGame([NotNull] IAgentWorkTracker agentWorkTracker)
        {
            ThrowHelper.ThrowIfNull(agentWorkTracker, nameof(agentWorkTracker));

            agentWorkTracker.Reset();
        }

        public static void PostfixWithLogging(Func<IAgentWorkTracker> getAgentWorkTracker)
        {
            try
            {
                ThrowHelper.ThrowIfNull(getAgentWorkTracker, nameof(getAgentWorkTracker));

                PatchAfterOnClickNewGame(getAgentWorkTracker());
            }
            catch (Exception ex)
            {
                Harmony_Patch.Instance.Logger.WriteException(ex);

                throw;
            }
        }

        /// <summary>Runs after the original OnClickNewGame method does to reset our agent work when the player starts a new game.</summary>
        [EntryPoint]
        [ExcludeFromCodeCoverage(Justification = Messages.UnityCodeCoverageJustification)]
        public static void Postfix()
        {
            PostfixWithLogging(() => Harmony_Patch.Instance.AgentWorkTracker);
        }
    }
}
