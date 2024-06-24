// SPDX-License-Identifier: MIT

#region

using System;
using System.Diagnostics.CodeAnalysis;
using Harmony;
using LobotomyCorporationMods.Common.Attributes;
using LobotomyCorporationMods.Common.Extensions;
using LobotomyCorporationMods.Common.Implementations;
using LobotomyCorporationMods.Common.Implementations.Adapters;
using LobotomyCorporationMods.Common.Interfaces.Adapters;

#endregion

namespace LobotomyCorporationMods.FreeCustomization.Patches
{
    [HarmonyPatch(typeof(AgentInfoWindow), "GenerateWindow")]
    public static class AgentInfoWindowPatchGenerateWindow
    {
        public static void PatchAfterGenerateWindow(this AgentInfoWindow instance,
            ICustomizingWindowAdapter customizingWindowAdapter)
        {
            Guard.Against.Null(instance, nameof(instance));
            Guard.Against.Null(customizingWindowAdapter, nameof(customizingWindowAdapter));

            var customizingWindow = instance.customizingWindow;
            customizingWindowAdapter.GameObject = customizingWindow;
            customizingWindowAdapter.OpenAppearanceWindow();
        }

        /// <summary>
        ///     Runs after opening the Agent window to automatically open the appearance window, since there's no reason to hide it
        ///     behind a button.
        /// </summary>
        [EntryPoint]
        [ExcludeFromCodeCoverage]
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
                Harmony_Patch.Instance.Logger.WriteToLog(ex);

                throw;
            }
        }
    }
}
