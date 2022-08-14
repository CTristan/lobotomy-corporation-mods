// SPDX-License-Identifier: MIT

using System;
using Harmony;

namespace LobotomyCorporationMods.FreeCustomization.Patches
{
    [HarmonyPatch(typeof(AgentInfoWindow), "GenerateWindow")]
    public static class AgentInfoWindowPatchGenerateWindow
    {
        /// <summary>
        ///     Runs after opening the Agent window to automatically open the appearance window, since there's no reason to hide it
        ///     behind a button.
        /// </summary>
        public static void Postfix()
        {
            try
            {
                AgentInfoWindow.currentWindow.customizingWindow.OpenAppearanceWindow();
            }
            catch (Exception ex)
            {
                Harmony_Patch.Instance.FileManager.WriteToLog(ex);

                throw;
            }
        }
    }
}
