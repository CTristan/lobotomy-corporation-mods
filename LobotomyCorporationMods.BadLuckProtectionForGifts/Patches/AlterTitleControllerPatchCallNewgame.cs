// SPDX-License-Identifier: MIT

using System;
using Harmony;

namespace LobotomyCorporationMods.BadLuckProtectionForGifts.Patches
{
    [HarmonyPatch(typeof(AlterTitleController), "CallNewgame")]
    public static class AlterTitleControllerPatchCallNewgame
    {
        /// <summary>
        ///     Runs after the original CallNewgame method does to reset our agent work when the player starts a new game.
        /// </summary>
        public static void Postfix()
        {
            try
            {
                Harmony_Patch.Instance.AgentWorkTracker.Reset();
            }
            catch (Exception ex)
            {
                Harmony_Patch.Instance.FileManager.WriteToLog(ex);

                throw;
            }
        }
    }
}
