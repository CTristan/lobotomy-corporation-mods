// SPDX-License-Identifier: MIT

#region

using System;
using System.Diagnostics.CodeAnalysis;
using Customizing;
using Harmony;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Attributes;
using LobotomyCorporationMods.Common.Constants;
using LobotomyCorporationMods.Common.Extensions;
using LobotomyCorporationMods.Common.Implementations;

#endregion

namespace LobotomyCorporationMods.CustomizationOverhaul.Patches
{
    [HarmonyPatch(typeof(CustomizingWindow), nameof(CustomizingWindow.Cancel))]
    public static class CustomizingWindowPatchCancel
    {
        public static void PatchAfterCancel([NotNull] this CustomizingWindow instance)
        {
            Guard.Against.Null(instance, nameof(instance));

            Harmony_Patch.DisableAllCustomUiComponents();
        }

        /// <summary>
        ///     Runs before canceling the Strengthen Employee window to save appearance data. Needs to run before the Cancel method because the Cancel method unloads the CurrentAgent
        ///     from the customizing window, so it would be too late for us to update the agent. This forcefully updates an agent's data because the game wasn't designed to allow you to
        ///     customize existing agents, so the game assumes the agent was already created before this step.
        /// </summary>
        // ReSharper disable InconsistentNaming
        [EntryPoint]
        [ExcludeFromCodeCoverage(Justification = Messages.UnityCodeCoverageJustification)]
        public static void Postfix([NotNull] CustomizingWindow __instance)
        {
            try
            {
                __instance.PatchAfterCancel();
            }
            catch (Exception ex)
            {
                Harmony_Patch.Instance.Logger.WriteException(ex);

                throw;
            }
        }
    }
}
