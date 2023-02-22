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
        public static ICustomizingWindowAdapter CustomizingWindowAdapter { get; set; } = new CustomizingWindowAdapter();
        public static IGameObjectAdapter GameObjectAdapter { get; set; } = new GameObjectAdapter();
        public static IAgentInfoWindowUiComponentsAdapter UiComponentsAdapter { get; set; } = new AgentInfoWindowUiComponentsAdapter();

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

                    // Make sure the customizing block is active so we can customize the agent
                    GameObjectAdapter.GameObject = agentInfoWindow.customizingBlock;
                    GameObjectAdapter.SetActive(true);

                    // Make the appearance control active
                    GameObjectAdapter.GameObject = agentInfoWindow.AppearanceActiveControl;
                    GameObjectAdapter.SetActive(true);

                    UiComponentsAdapter.GameObject = agentInfoWindow.UIComponents;
                    UiComponentsAdapter.SetData(customizingWindow.CurrentData);

                    CustomizingWindowAdapter.GameObject = agentInfoWindow.customizingWindow;
                    CustomizingWindowAdapter.OpenAppearanceWindow();
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
