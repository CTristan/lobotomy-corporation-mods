using System;
using Harmony;

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
                Harmony_Patch.GetAgentWorkTracker().Save();
            }
            catch (Exception ex)
            {
                Harmony_Patch.GetFileManager().WriteToLog(ex);

                throw;
            }
        }
    }
}
