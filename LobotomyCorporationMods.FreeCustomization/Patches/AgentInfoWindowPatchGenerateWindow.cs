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
    [HarmonyPatch(typeof(AgentInfoWindow), nameof(AgentInfoWindow.GenerateWindow))]
    public static class AgentInfoWindowPatchGenerateWindow
    {
        public static void PatchAfterGenerateWindow(
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

        /// <summary>Runs after opening the Agent window to automatically open the appearance window, since there's no reason to hide it behind a button.</summary>
        [EntryPoint]
        [ExcludeFromCodeCoverage(Justification = Messages.UnityCodeCoverageJustification)]
        public static void Postfix()
        {
            try
            {
                // GenerateWindow is a static method, so we can't get an instance of it through Harmony.
                var agentInfoWindow = AgentInfoWindow.currentWindow;

                agentInfoWindow.PatchAfterGenerateWindow();
            }
            catch (Exception ex)
            {
                Harmony_Patch.Instance.Logger.WriteException(ex);

                throw;
            }
        }
    }
}
