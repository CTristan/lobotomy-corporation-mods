// SPDX-License-Identifier: MIT

#region

using System;
using System.Diagnostics.CodeAnalysis;
using Customizing;
using Harmony;
using LobotomyCorporationMods.Common.Attributes;
using LobotomyCorporationMods.Common.Implementations.Adapters;
using LobotomyCorporationMods.Common.Interfaces.Adapters;

#endregion

namespace LobotomyCorporationMods.FreeCustomization.Patches
{
    [HarmonyPatch(typeof(AgentInfoWindow), "EnforcementWindow")]
    public static class AgentInfoWindowPatchEnforcementWindow
    {
        public static void PatchAfterEnforcementWindow(this AgentInfoWindow instance, ICustomizingWindowAdapter customizingWindowAdapter, IGameObjectAdapter gameObjectAdapter,
            IAgentInfoWindowUiComponentsAdapter uiComponentsAdapter)
        {
            if (instance is null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            if (customizingWindowAdapter is null)
            {
                throw new ArgumentNullException(nameof(customizingWindowAdapter));
            }

            if (gameObjectAdapter is null)
            {
                throw new ArgumentNullException(nameof(gameObjectAdapter));
            }

            if (uiComponentsAdapter is null)
            {
                throw new ArgumentNullException(nameof(uiComponentsAdapter));
            }

            if (instance.customizingWindow.CurrentData is not null)
            {
                var customizingWindow = CustomizingWindow.CurrentWindow;

                // Make sure the customizing block is active so we can customize the agent
                gameObjectAdapter.GameObject = instance.customizingBlock;
                gameObjectAdapter.SetActive(true);

                // Make the appearance control active
                gameObjectAdapter.GameObject = instance.AppearanceActiveControl;
                gameObjectAdapter.SetActive(true);

                uiComponentsAdapter.GameObject = instance.UIComponents;
                uiComponentsAdapter.SetData(customizingWindow.CurrentData);

                customizingWindowAdapter.GameObject = instance.customizingWindow;
                customizingWindowAdapter.OpenAppearanceWindow();
            }
        }

        /// <summary>
        ///     Runs after opening the Strengthen Agent window to open the appearance window.
        /// </summary>
        // EnforcementWindow is a static method, so we can't get an instance of the AgentInfoWindow through Harmony.
        [EntryPoint]
        [ExcludeFromCodeCoverage]
        public static void Postfix()
        {
            try
            {
                var agentInfoWindow = AgentInfoWindow.currentWindow;

                agentInfoWindow.PatchAfterEnforcementWindow(new CustomizingWindowAdapter(), new GameObjectAdapter(), new AgentInfoWindowUiComponentsAdapter());
            }
            catch (Exception ex)
            {
                Harmony_Patch.Instance.Logger.WriteToLog(ex);

                throw;
            }
        }
    }
}
