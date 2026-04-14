// SPDX-License-Identifier: MIT

#region

using System;
using System.Diagnostics.CodeAnalysis;
using Customizing;
using Harmony;
using JetBrains.Annotations;
using LobotomyCorporation.Mods.Common;

#endregion

namespace LobotomyCorporationMods.FreeCustomization.Patches
{
    [HarmonyPatch(typeof(CustomizingWindow), GameMethods.CustomizingWindow.ReviseOpenAction)]
    public static class CustomizingWindowPatchReviseOpenAction
    {
        public static void PatchAfterReviseOpenAction(
            [NotNull] this CustomizingWindow instance,
            [NotNull] AgentModel agent
        )
        {
            ThrowHelper.ThrowIfNull(instance, nameof(instance));
            ThrowHelper.ThrowIfNull(agent, nameof(agent));

            instance.LoadAgentData(agent);
        }

        /// <summary>Runs after opening the Strengthen Agent window to set the appearance data for the customization window.</summary>
        // ReSharper disable InconsistentNaming
        [EntryPoint]
        [ExcludeFromCodeCoverage(Justification = Messages.UnityCodeCoverageJustification)]
        public static void Postfix(
            [NotNull] CustomizingWindow __instance,
            [NotNull] AgentModel agent
        )
        {
            try
            {
                __instance.PatchAfterReviseOpenAction(agent);
            }
            catch (Exception ex)
            {
                Harmony_Patch.Instance.Logger.WriteException(ex);

                throw;
            }
        }
        // ReSharper disable InconsistentNaming
    }
}
