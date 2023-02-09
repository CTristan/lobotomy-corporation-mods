// SPDX-License-Identifier: MIT

using System;
using Harmony;
using LobotomyCorporationMods.FreeCustomization.Extensions;

namespace LobotomyCorporationMods.FreeCustomization.Patches
{
    [HarmonyPatch(typeof(AgentInfoWindow), "EnforcementWindow")]
    public static class AgentInfoWindowPatchEnforcementWindow
    {
        /// <summary>
        ///     Runs after opening the Strengthen Agent window to open the appearance window.
        /// </summary>
        // EnforcementWindow is a static method, so we can't get an instance of the AgentInfoWindow through Harmony.
        public static void Postfix()
        {
            try
            {
                var agentInfoWindow = AgentInfoWindow.currentWindow;

                if (agentInfoWindow.customizingWindow.CurrentData != null)
                {
                    agentInfoWindow.OpenAppearanceWindow();
                }
            }
            catch (Exception ex)
            {
                Harmony_Patch.Instance.Logger.WriteToLog(ex);

                throw;
            }
        }
    }
}
