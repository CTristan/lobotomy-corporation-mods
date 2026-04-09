// SPDX-License-Identifier: MIT

#region

using System;
using System.Diagnostics.CodeAnalysis;
using Harmony;
using JetBrains.Annotations;
using LobotomyCorporation.Mods.Common.Attributes;
using LobotomyCorporation.Mods.Common.Constants;
using LobotomyCorporation.Mods.Common.Extensions;
using LobotomyCorporation.Mods.Common.Implementations;
using LobotomyCorporation.Mods.Common.Implementations.Facades;
using LobotomyCorporation.Mods.Common.Interfaces.Adapters;
using LobotomyCorporation.Mods.Common.Interfaces.Adapters.BaseClasses;

#endregion

namespace LobotomyCorporationMods.FreeCustomization.Patches
{
    [HarmonyPatch(typeof(AgentInfoWindow), nameof(AgentInfoWindow.EnforcementWindow))]
    public static class AgentInfoWindowPatchEnforcementWindow
    {
        public static void PatchAfterEnforcementWindow(
            [NotNull] this AgentInfoWindow instance,
            [CanBeNull]
                IAgentInfoWindowUiComponentsTestAdapter agentInfoWindowUiComponentsTestAdapter =
                null,
            [CanBeNull] ICustomizingWindowTestAdapter customizingWindowTestAdapter = null,
            [CanBeNull] IGameObjectTestAdapter gameObjectTestAdapter = null
        )
        {
            ThrowHelper.ThrowIfNull(instance, nameof(instance));

            instance.OpenAppearancePanel(
                agentInfoWindowUiComponentsTestAdapter,
                customizingWindowTestAdapter,
                gameObjectTestAdapter
            );
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

                agentInfoWindow.PatchAfterEnforcementWindow();
            }
            catch (Exception ex)
            {
                Harmony_Patch.Instance.Logger.WriteException(ex);

                throw;
            }
        }
    }
}
