// SPDX-License-Identifier: MIT

#region

using System;
using System.Diagnostics.CodeAnalysis;
using Customizing;
using Harmony;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Attributes;
using LobotomyCorporationMods.Common.Constants;
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
            [NotNull] ICustomizingWindowTestAdapter customizingWindowTestAdapter,
            [NotNull] IGameObjectTestAdapter gameObjectTestAdapter,
            [NotNull] IAgentInfoWindowUiComponentsTestAdapter uiComponentsTestAdapter)
        {
            Guard.Against.Null(instance, nameof(instance));
            Guard.Against.Null(gameObjectTestAdapter, nameof(gameObjectTestAdapter));
            Guard.Against.Null(uiComponentsTestAdapter, nameof(uiComponentsTestAdapter));
            Guard.Against.Null(customizingWindowTestAdapter, nameof(customizingWindowTestAdapter));

            var customizingWindow = CustomizingWindow.CurrentWindow;

            // Make sure the customizing block is active so we can customize the agent
            gameObjectTestAdapter.GameObject = instance.customizingBlock;
            gameObjectTestAdapter.SetActive(true);

            // Make the appearance control active
            gameObjectTestAdapter.GameObject = instance.AppearanceActiveControl;
            gameObjectTestAdapter.SetActive(true);

            uiComponentsTestAdapter.GameObject = instance.UIComponents;
            uiComponentsTestAdapter.SetData(customizingWindow.CurrentData);

            customizingWindowTestAdapter.GameObject = instance.customizingWindow;
            customizingWindowTestAdapter.OpenAppearanceWindow();
        }

        /// <summary>Runs after opening the Strengthen Agent window to open the appearance window.</summary>
        [EntryPoint]
        [ExcludeFromCodeCoverage(Justification = Messages.UnityCodeCoverageJustification)]
        public static void Postfix()
        {
            try
            {
                // EnforcementWindow is a static method, so we can't get an instance of the AgentInfoWindow through Harmony.
                var agentInfoWindow = AgentInfoWindow.currentWindow;

                agentInfoWindow.PatchAfterEnforcementWindow(new CustomizingWindowTestAdapter(), new GameObjectTestAdapter(), new AgentInfoWindowUiComponentsTestAdapter());
            }
            catch (Exception ex)
            {
                Harmony_Patch.Instance.Logger.WriteException(ex);

                throw;
            }
        }
    }
}
