// SPDX-License-Identifier: MIT

#region

using System;
using System.Diagnostics.CodeAnalysis;
using Customizing;
using Harmony;
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
        [SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores")]
        [SuppressMessage("Style", "IDE1006:Naming Styles")]
        // ReSharper disable once InconsistentNaming
        public static void Postfix(CustomizingWindow __instance, AgentModel agent)
        {
            try
            {
                if (__instance is null)
                {
                    throw new ArgumentNullException(nameof(__instance));
                }

                if (agent is null)
                {
                    throw new ArgumentNullException(nameof(agent));
                }

                __instance.CurrentData.agentName = agent._agentName;
                __instance.CurrentData.CustomName = agent.name;
                __instance.CurrentData.appearance = agent.GetAppearanceData();
            }
            catch (Exception ex)
            {
                Harmony_Patch.Instance.Logger.WriteToLog(ex);

                throw;
            }
        }
    }
}
