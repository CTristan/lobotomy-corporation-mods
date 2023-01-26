// SPDX-License-Identifier: MIT

using System;
using System.Diagnostics.CodeAnalysis;
using Customizing;
using Harmony;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Extensions;
using LobotomyCorporationMods.Common.Implementations;
using LobotomyCorporationMods.FreeCustomization.Extensions;

namespace LobotomyCorporationMods.FreeCustomization.Patches
{
    [HarmonyPatch(typeof(CustomizingWindow), "ReviseOpenAction")]
    public static class CustomizingWindowPatchReviseOpenAction
    {
        /// <summary>
        ///     Runs after opening the Strengthen Agent window to set the appearance data for the customization window.
        /// </summary>
        [SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores")]
        // ReSharper disable once InconsistentNaming
        public static void Postfix([NotNull] CustomizingWindow __instance, [NotNull] AgentModel agent)
        {
            try
            {
                Guard.Against.Null(__instance, nameof(__instance));
                Guard.Against.Null(agent, nameof(agent));

                __instance.CurrentData.agentName = agent._agentName;
                __instance.CurrentData.appearance = agent.GetAppearanceData();
                __instance.CurrentData.CustomName = agent.name;
                AgentInfoWindow.currentWindow.UIComponents.SetData(CustomizingWindow.CurrentWindow.CurrentData);
            }
            catch (Exception ex)
            {
                Harmony_Patch.Instance.FileManager.WriteToLog(ex);

                throw;
            }
        }
    }
}
