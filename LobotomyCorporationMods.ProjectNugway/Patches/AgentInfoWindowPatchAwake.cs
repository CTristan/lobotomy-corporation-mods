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
using LobotomyCorporationMods.ProjectNugway.Interfaces;

#endregion

namespace LobotomyCorporationMods.ProjectNugway.Patches
{
    [HarmonyPatch(typeof(AgentInfoWindow), PrivateMethods.AgentInfoWindow.Awake)]
    public static class AgentInfoWindowPatchAwake
    {
        /// <summary>
        ///     Runs after the AgentInfoWindow has been initialized to disable any custom UI components. Needed because otherwise the custom UI components will be visible when selecting
        ///     different agents.
        /// </summary>
        /// <param name="instance">An instance of AgentInfoWindow</param>
        public static void PatchAfterAwake([NotNull] this AgentInfoWindow instance,
            [NotNull] IUiController uiController)
        {
            Guard.Against.Null(instance, nameof(instance));
            Guard.Against.Null(uiController, nameof(uiController));

            if (GameManager.currentGameManager.state != GameState.STOP)
            {
                uiController.DisableAllCustomUiComponents();
            }
        }

        [EntryPoint]
        [ExcludeFromCodeCoverage(Justification = Messages.UnityCodeCoverageJustification)]
        public static void Postfix()
        {
            try
            {
                // EnforcementWindow is a static method, so we can't get an instance of the AgentInfoWindow through Harmony.
                var agentInfoWindow = AgentInfoWindow.currentWindow;

                agentInfoWindow.PatchAfterAwake(Harmony_Patch.Instance.UiController);
            }
            catch (Exception ex)
            {
                Harmony_Patch.Instance.Logger.LogError(ex);

                throw;
            }
        }
    }
}
