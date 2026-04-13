// SPDX-License-Identifier: MIT

#region

using System;
using System.Diagnostics.CodeAnalysis;
using Harmony;
using JetBrains.Annotations;
using LobotomyCorporation.Mods.Common;

#endregion

namespace LobotomyCorporationMods.FreeCustomization.Patches
{
    [HarmonyPatch(typeof(AgentInfoWindow), nameof(AgentInfoWindow.EnforcementWindow))]
    public static class AgentInfoWindowPatchEnforcementWindow
    {
        public static void PatchAfterEnforcementWindow(
            [NotNull] this AgentInfoWindow instance,
            [CanBeNull]
                IAgentInfoWindowUiComponentsInternals agentInfoWindowUiComponentsInternals = null,
            [CanBeNull] ICustomizingWindowInternals customizingWindowInternals = null,
            [CanBeNull] IGameObjectInternals customizingBlockInternals = null,
            [CanBeNull] IGameObjectInternals appearanceControlInternals = null
        )
        {
            ThrowHelper.ThrowIfNull(instance, nameof(instance));

            instance.OpenAppearancePanel(
                agentInfoWindowUiComponentsInternals,
                customizingWindowInternals,
                customizingBlockInternals,
                appearanceControlInternals
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
