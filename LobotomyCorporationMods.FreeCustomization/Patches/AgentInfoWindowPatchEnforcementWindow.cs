// SPDX-License-Identifier: MIT

#region

using System;
using System.Diagnostics.CodeAnalysis;
using Harmony;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Attributes;
using LobotomyCorporationMods.Common.Constants;
using LobotomyCorporationMods.Common.Implementations;
using LobotomyCorporationMods.Common.Implementations.Facades;
using LobotomyCorporationMods.Common.Interfaces.Adapters;
using LobotomyCorporationMods.Common.Interfaces.Adapters.BaseClasses;

#endregion

namespace LobotomyCorporationMods.FreeCustomization.Patches
{
    [HarmonyPatch(typeof(AgentInfoWindow), nameof(AgentInfoWindow.EnforcementWindow))]
    public static class AgentInfoWindowPatchEnforcementWindow
    {
        public static void PatchAfterEnforcementWindow([NotNull] this AgentInfoWindow instance,
            [CanBeNull] IAgentInfoWindowUiComponentsTestAdapter agentInfoWindowUiComponentsTestAdapter = null,
            [CanBeNull] ICustomizingWindowTestAdapter customizingWindowTestAdapter = null,
            [CanBeNull] IGameObjectTestAdapter gameObjectTestAdapter = null)
        {
            ThrowHelper.ThrowIfNull(instance, nameof(instance));

            instance.OpenAppearancePanel(agentInfoWindowUiComponentsTestAdapter, customizingWindowTestAdapter, gameObjectTestAdapter);
        }

        public static void PostfixWithLogging(Func<AgentInfoWindow> getAgentInfoWindow)
        {
            try
            {
                ThrowHelper.ThrowIfNull(getAgentInfoWindow, nameof(getAgentInfoWindow));

                // EnforcementWindow is a static method, so we can't get an instance of the AgentInfoWindow through Harmony.
                var agentInfoWindow = getAgentInfoWindow();

                agentInfoWindow.PatchAfterEnforcementWindow();
            }
            catch (Exception ex)
            {
                Harmony_Patch.Instance.Logger.WriteException(ex);

                throw;
            }
        }

        /// <summary>Runs after opening the Strengthen Agent window to open the appearance window.</summary>
        [EntryPoint]
        [ExcludeFromCodeCoverage(Justification = Messages.UnityCodeCoverageJustification)]
        public static void Postfix()
        {
            PostfixWithLogging(() => AgentInfoWindow.currentWindow);
        }
    }
}
