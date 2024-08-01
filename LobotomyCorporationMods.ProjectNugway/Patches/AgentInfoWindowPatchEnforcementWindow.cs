// SPDX-License-Identifier: MIT

#region

using System;
using System.Diagnostics.CodeAnalysis;
using Harmony;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Attributes;
using LobotomyCorporationMods.Common.Constants;
using LobotomyCorporationMods.Common.Extensions;
using LobotomyCorporationMods.Common.Implementations;
using LobotomyCorporationMods.Common.Implementations.Facades;
using LobotomyCorporationMods.Common.Interfaces.Adapters;
using LobotomyCorporationMods.Common.Interfaces.Adapters.BaseClasses;
using LobotomyCorporationMods.ProjectNugway.Interfaces;

#endregion

namespace LobotomyCorporationMods.ProjectNugway.Patches
{
    [HarmonyPatch(typeof(AgentInfoWindow), nameof(AgentInfoWindow.EnforcementWindow))]
    public static class AgentInfoWindowPatchEnforcementWindow
    {
        public static void PatchAfterEnforcementWindow([NotNull] this AgentInfoWindow instance,
            [NotNull] IUiController uiController,
            [CanBeNull] IAgentInfoWindowUiComponentsTestAdapter agentInfoWindowUiComponentsTestAdapter = null,
            [CanBeNull] ICustomizingWindowTestAdapter customizingWindowTestAdapter = null,
            [CanBeNull] IGameObjectTestAdapter gameObjectTestAdapter = null)
        {
            Guard.Against.Null(instance, nameof(instance));
            Guard.Against.Null(uiController, nameof(uiController));

            instance.OpenAppearancePanel(agentInfoWindowUiComponentsTestAdapter, customizingWindowTestAdapter, gameObjectTestAdapter);

            uiController.DisplayLoadPresetButton();
            uiController.DisplaySavePresetButton();
        }

        /// <summary>Runs after opening the Strengthen Agent window to force it to open the appearance window.</summary>
        [EntryPoint]
        [ExcludeFromCodeCoverage(Justification = Messages.UnityCodeCoverageJustification)]
        public static void Postfix()
        {
            try
            {
                // EnforcementWindow is a static method, so we can't get an instance of the AgentInfoWindow through Harmony.
                var agentInfoWindow = AgentInfoWindow.currentWindow;

                agentInfoWindow.PatchAfterEnforcementWindow(Harmony_Patch.Instance.UiController);
            }
            catch (Exception ex)
            {
                Harmony_Patch.Instance.Logger.LogError(ex);

                throw;
            }
        }
    }
}
