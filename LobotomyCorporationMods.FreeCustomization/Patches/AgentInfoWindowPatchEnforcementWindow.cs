// SPDX-License-Identifier: MIT

#region

using System;
using Customizing;
using Harmony;
using LobotomyCorporationMods.Common.Implementations.Adapters;
using LobotomyCorporationMods.Common.Interfaces.Adapters;

#endregion

namespace LobotomyCorporationMods.FreeCustomization.Patches
{
    [HarmonyPatch(typeof(AgentInfoWindow), "EnforcementWindow")]
    public static class AgentInfoWindowPatchEnforcementWindow
    {
        public static ICustomizingWindowAdapter AgentInfoWindowCustomizingWindowAdapter { private get; set; }
        public static IGameObjectAdapter GameObjectAppearanceActiveControlAdapter { private get; set; }
        public static IGameObjectAdapter GameObjectCustomizingBlockAdapter { private get; set; }
        public static IAgentInfoWindowUiComponentsAdapter InfoWindowUiComponentsAdapter { private get; set; }

        /// <summary>
        ///     Runs after opening the Strengthen Agent window to open the appearance window.
        /// </summary>
        // EnforcementWindow is a static method, so we can't get an instance of the AgentInfoWindow through Harmony.
        public static void Postfix()
        {
            try
            {
                var agentInfoWindow = AgentInfoWindow.currentWindow;

                if (agentInfoWindow.customizingWindow.CurrentData is not null)
                {
                    var customizingWindow = CustomizingWindow.CurrentWindow;

                    GameObjectCustomizingBlockAdapter ??= new GameObjectAdapter(agentInfoWindow.customizingBlock);
                    GameObjectCustomizingBlockAdapter.SetActive(true);

                    GameObjectAppearanceActiveControlAdapter ??= new GameObjectAdapter(agentInfoWindow.AppearanceActiveControl);
                    GameObjectAppearanceActiveControlAdapter.SetActive(true);

                    InfoWindowUiComponentsAdapter ??= new AgentInfoWindowUiComponentsAdapter(agentInfoWindow.UIComponents);
                    InfoWindowUiComponentsAdapter.SetData(customizingWindow.CurrentData);

                    AgentInfoWindowCustomizingWindowAdapter ??= new CustomizingWindowAdapter(agentInfoWindow.customizingWindow);
                    AgentInfoWindowCustomizingWindowAdapter.OpenAppearanceWindow();
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
