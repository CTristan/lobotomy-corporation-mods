// SPDX-License-Identifier: MIT

#region

using System;
using Harmony;

#endregion

namespace LobotomyCorporationMods.BadLuckProtectionForGifts.Patches
{
    [HarmonyPatch(typeof(GameSceneController), "OnStageStart")]
    public static class GameSceneControllerPatchOnStageStart
    {
        /// <summary>
        ///     Runs after the original OnStageStart method to reset our tracker progress. We reset the progress on restart
        ///     because it doesn't make sense that an agent would remember their creature experience if the day is reset.
        /// </summary>
        public static void Postfix()
        {
            try
            {
                Harmony_Patch.Instance.AgentWorkTracker.Load();
            }
            catch (Exception ex)
            {
                Harmony_Patch.Instance.Logger.WriteToLog(ex);

                throw;
            }
        }
    }
}
