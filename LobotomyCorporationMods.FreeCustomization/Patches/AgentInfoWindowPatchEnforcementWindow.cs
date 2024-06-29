// SPDX-License-Identifier: MIT

#region

using System;
using System.Diagnostics.CodeAnalysis;
using Customizing;
using Harmony;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Attributes;
using LobotomyCorporationMods.Common.Extensions;
using LobotomyCorporationMods.Common.Implementations;
using LobotomyCorporationMods.Common.Implementations.Adapters;
using LobotomyCorporationMods.Common.Interfaces.Adapters;

#endregion

namespace LobotomyCorporationMods.FreeCustomization.Patches
{
    [HarmonyPatch(typeof(AgentInfoWindow), nameof(AgentInfoWindow.EnforcementWindow))]
    public static class AgentInfoWindowPatchEnforcementWindow
    {
        public static void PatchAfterEnforcementWindow([NotNull] this AgentInfoWindow instance,
            [NotNull] ICustomizingWindowAdapter customizingWindowAdapter,
            [NotNull] IGameObjectAdapter gameObjectAdapter,
            [NotNull] IAgentInfoWindowUiComponentsAdapter uiComponentsAdapter)
        {
            Guard.Against.Null(instance, nameof(instance));
            Guard.Against.Null(gameObjectAdapter, nameof(gameObjectAdapter));
            Guard.Against.Null(uiComponentsAdapter, nameof(uiComponentsAdapter));
            Guard.Against.Null(customizingWindowAdapter, nameof(customizingWindowAdapter));

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

        /// <summary>Runs after opening the Strengthen Agent window to open the appearance window.</summary>
        [EntryPoint]
        [ExcludeFromCodeCoverage]
        public static void Postfix()
        {
            try
            {
                // EnforcementWindow is a static method, so we can't get an instance of the AgentInfoWindow through Harmony.
                var agentInfoWindow = AgentInfoWindow.currentWindow;

                agentInfoWindow.PatchAfterEnforcementWindow(new CustomizingWindowAdapter(), new GameObjectAdapter(), new AgentInfoWindowUiComponentsAdapter());
            }
            catch (Exception ex)
            {
                Harmony_Patch.Instance.Logger.WriteException(ex);

                throw;
            }
        }
    }
}
