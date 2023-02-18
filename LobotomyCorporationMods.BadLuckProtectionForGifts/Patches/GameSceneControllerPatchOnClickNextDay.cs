// SPDX-License-Identifier: MIT

#region

using System;
using Harmony;

#endregion

namespace LobotomyCorporationMods.BadLuckProtectionForGifts.Patches
{
    [HarmonyPatch(typeof(GameSceneController), "OnClickNextDay")]
    public static class GameSceneControllerPatchOnClickNextDay
    {
        /// <summary>
        ///     Runs after the original OnClickNextDay method to save our tracker progress. We only save when going to the next
        ///     day because it doesn't make sense that an agent would remember their creature experience if the day is reset.
        /// </summary>
        public static void Postfix()
        {
            try
            {
                Harmony_Patch.Instance.AgentWorkTracker.Save();
            }
            catch (Exception ex)
            {
                Harmony_Patch.Instance.Logger.WriteToLog(ex);

                throw;
            }
        }
    }
}
