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
    [HarmonyPatch(typeof(NewTitleScript), "OnClickNewGame")]
    public static class NewTitleScriptPatchOnClickNewGame
    {
        public static void PatchAfterOnClickNewGame(IAgentWorkTracker agentWorkTracker)
        {
            Guard.Against.Null(agentWorkTracker, nameof(agentWorkTracker));

            agentWorkTracker.Reset();
        }

        /// <summary>
        ///     Runs after the original OnClickNewGame method does to reset our agent work when the player starts a new game.
        /// </summary>
        [EntryPoint]
        [ExcludeFromCodeCoverage]
        public static void Postfix()
        {
            try
            {
                PatchAfterOnClickNewGame(Harmony_Patch.Instance.AgentWorkTracker);
            }
            catch (Exception ex)
            {
                Harmony_Patch.Instance.Logger.WriteToLog(ex);

                throw;
            }
        }
    }
}
