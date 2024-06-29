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
using LobotomyCorporationMods.Common.Implementations.Adapters;
using LobotomyCorporationMods.Common.Interfaces.Adapters;

#endregion

namespace LobotomyCorporationMods.FreeCustomization.Patches
{
    [HarmonyPatch(typeof(AgentInfoWindow), nameof(AgentInfoWindow.GenerateWindow))]
    public static class AgentInfoWindowPatchGenerateWindow
    {
        public static void PatchAfterGenerateWindow([NotNull] this AgentInfoWindow instance,
            [NotNull] ICustomizingWindowAdapter customizingWindowAdapter)
        {
            Guard.Against.Null(instance, nameof(instance));
            Guard.Against.Null(customizingWindowAdapter, nameof(customizingWindowAdapter));

            var customizingWindow = instance.customizingWindow;
            customizingWindowAdapter.GameObject = customizingWindow;
            customizingWindowAdapter.OpenAppearanceWindow();
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

                agentInfoWindow.PatchAfterGenerateWindow(new CustomizingWindowAdapter());
            }
            catch (Exception ex)
            {
                Harmony_Patch.Instance.Logger.WriteException(ex);

                throw;
            }
        }
    }
}
