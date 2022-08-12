using System;
using Harmony;

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
                Harmony_Patch.GetAgentWorkTracker().Load();
            }
            catch (Exception ex)
            {
                Harmony_Patch.GetFileManager().WriteToLog(ex);

                throw;
            }
        }
    }
}