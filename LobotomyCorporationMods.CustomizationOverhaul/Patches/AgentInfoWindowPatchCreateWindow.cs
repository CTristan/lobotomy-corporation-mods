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

#endregion

namespace LobotomyCorporationMods.CustomizationOverhaul.Patches
{
    [HarmonyPatch(typeof(AgentInfoWindow), nameof(AgentInfoWindow.CreateWindow))]
    public static class AgentInfoWindowPatchCreateWindow
    {
        public static void PatchAfterCreateWindow([NotNull] this AgentInfoWindow instance)
        {
            Guard.Against.Null(instance, nameof(instance));

            if (GameManager.currentGameManager.state != GameState.STOP)
            {
                Harmony_Patch.Instance.UiController.DisableAllCustomUiComponents();
            }
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

                agentInfoWindow.PatchAfterCreateWindow();
            }
            catch (Exception ex)
            {
                Harmony_Patch.Instance.Logger.LogError(ex);

                throw;
            }
        }
    }
}
