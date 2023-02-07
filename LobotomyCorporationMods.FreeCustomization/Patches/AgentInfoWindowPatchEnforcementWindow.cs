// SPDX-License-Identifier: MIT

using System;
using Customizing;
using Harmony;

namespace LobotomyCorporationMods.FreeCustomization.Patches
{
    [HarmonyPatch(typeof(AgentInfoWindow), "EnforcementWindow")]
    public static class AgentInfoWindowPatchEnforcementWindow
    {
        /// <summary>
        ///     Runs after opening the Strengthen Agent window to open the appearance window.
        /// </summary>
        // EnforcementWindow is a static method, so we can't get an instance of it through Harmony.
        public static void Postfix()
        {
            try
            {
                if (AgentInfoWindow.currentWindow.customizingWindow.CurrentData == null)
                {
                    return;
                }

                AgentInfoWindow.currentWindow.customizingBlock.SetActive(true);
                AgentInfoWindow.currentWindow.AppearanceActiveControl.SetActive(true);
                AgentInfoWindow.currentWindow.UIComponents.SetData(CustomizingWindow.CurrentWindow.CurrentData);
                AgentInfoWindow.currentWindow.customizingWindow.OpenAppearanceWindow();
            }
            catch (Exception ex)
            {
                Harmony_Patch.Instance.Logger.WriteToLog(ex);

                throw;
            }
        }
    }
}
