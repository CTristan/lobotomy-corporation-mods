// SPDX-License-Identifier: MIT

#region

using System;
using System.Diagnostics.CodeAnalysis;
using Customizing;
using Harmony;
using LobotomyCorporationMods.Common.Attributes;
using LobotomyCorporationMods.FreeCustomization.Extensions;

#endregion

namespace LobotomyCorporationMods.FreeCustomization.Patches
{
    [HarmonyPatch(typeof(CustomizingWindow), "ReviseOpenAction")]
    public static class CustomizingWindowPatchReviseOpenAction
    {
        /// <summary>
        ///     Runs after opening the Strengthen Agent window to set the appearance data for the customization window.
        /// </summary>
        // ReSharper disable InconsistentNaming
        [EntryPoint]
        [ExcludeFromCodeCoverage]
        public static void Postfix(CustomizingWindow __instance, AgentModel agent)
        {
            try
            {
                __instance.PatchAfterReviseOpenAction(agent);
            }
            catch (Exception ex)
            {
                Harmony_Patch.Instance.Logger.WriteToLog(ex);

                throw;
            }
        }

        public static void PatchAfterReviseOpenAction(this CustomizingWindow instance, AgentModel agent)
        {
            if (instance is null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            if (agent is null)
            {
                throw new ArgumentNullException(nameof(agent));
            }

            instance.CurrentData.agentName = agent._agentName;
            instance.CurrentData.CustomName = agent.name;
            instance.CurrentData.appearance = agent.GetAppearanceData();
        }
    }
}
