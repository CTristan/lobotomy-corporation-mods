// SPDX-License-Identifier: MIT

#region

using System;
using Harmony;
using LobotomyCorporationMods.Common.Implementations.Adapters;
using LobotomyCorporationMods.Common.Interfaces.Adapters;

#endregion

namespace LobotomyCorporationMods.FreeCustomization.Patches
{
    [HarmonyPatch(typeof(AgentInfoWindow), "GenerateWindow")]
    public static class AgentInfoWindowPatchGenerateWindow
    {
        public static ICustomizingWindowAdapter Adapter { get; set; } = new CustomizingWindowAdapter();

        /// <summary>
        ///     Runs after opening the Agent window to automatically open the appearance window, since there's no reason to hide it
        ///     behind a button.
        /// </summary>
        // GenerateWindow is a static method, so we can't get an instance of it through Harmony.
        public static void Postfix()
        {
            try
            {
                var customizingWindow = AgentInfoWindow.currentWindow.customizingWindow;

                Adapter.GameObject = customizingWindow;
                Adapter.OpenAppearanceWindow();
            }
            catch (Exception ex)
            {
                Harmony_Patch.Instance.Logger.WriteToLog(ex);

                throw;
            }
        }
    }
}
