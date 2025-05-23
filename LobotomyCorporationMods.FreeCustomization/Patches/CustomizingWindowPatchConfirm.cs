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
using LobotomyCorporationMods.Common.Implementations.Facades;
using LobotomyCorporationMods.Common.Interfaces.Adapters;

#endregion

namespace LobotomyCorporationMods.FreeCustomization.Patches
{
    [HarmonyPatch(typeof(CustomizingWindow), nameof(CustomizingWindow.Confirm))]
    public static class CustomizingWindowPatchConfirm
    {
        public static void PatchBeforeConfirm([NotNull] this CustomizingWindow instance,
            [CanBeNull] IAgentLayerTestAdapter agentLayerTestAdapter = null,
            [CanBeNull] IWorkerSpriteManagerTestAdapter workerSpriteManagerTestAdapter = null)
        {
            Guard.Against.Null(instance, nameof(instance));

            instance.SaveAppearanceData(agentLayerTestAdapter, workerSpriteManagerTestAdapter);
        }

        /// <summary>
        ///     Runs before confirming the Strengthen Employee window to save appearance data. Needs to run before the Confirm
        ///     method because the Confirm method unloads the CurrentAgent
        ///     from the customizing window, so it would be too late for us to update the agent. This forcefully updates an agent's
        ///     data because the game wasn't designed to allow you to
        ///     customize existing agents, so the game assumes the agent was already created before this step.
        /// </summary>
        // ReSharper disable InconsistentNaming
        [EntryPoint]
        [ExcludeFromCodeCoverage(Justification = Messages.UnityCodeCoverageJustification)]
        public static void Prefix([NotNull] CustomizingWindow __instance)
        {
            try
            {
                __instance.PatchBeforeConfirm();
            }
            catch (Exception ex)
            {
                Harmony_Patch.Instance.Logger.LogException(ex);

                throw;
            }
        }
    }
}
