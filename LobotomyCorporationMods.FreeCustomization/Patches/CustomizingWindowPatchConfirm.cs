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
using LobotomyCorporationMods.Common.Implementations.Adapters;
using LobotomyCorporationMods.Common.Interfaces.Adapters;
using LobotomyCorporationMods.FreeCustomization.Extensions;

#endregion

namespace LobotomyCorporationMods.FreeCustomization.Patches
{
    [HarmonyPatch(typeof(CustomizingWindow), nameof(CustomizingWindow.Confirm))]
    public static class CustomizingWindowPatchConfirm
    {
        public static void PatchBeforeConfirm([NotNull] this CustomizingWindow instance,
            [NotNull] IAgentLayerTestAdapter agentLayerTestAdapter,
            [NotNull] IWorkerSpriteManagerTestAdapter workerSpriteManagerTestAdapter)
        {
            Guard.Against.Null(instance, nameof(instance));
            Guard.Against.Null(agentLayerTestAdapter, nameof(agentLayerTestAdapter));
            Guard.Against.Null(workerSpriteManagerTestAdapter, nameof(workerSpriteManagerTestAdapter));

            // The original method does this and other things for newly-generated agents, so we only want to do this
            // when we're updating existing agents.
            if (instance.CurrentWindowType == CustomizingType.GENERATE)
            {
                return;
            }

            instance.SaveAgentAppearance();
            instance.RenameAgent();
            instance.CurrentData.appearance.SetResrouceData();

            workerSpriteManagerTestAdapter.GameObject = WorkerSpriteManager.instance;
            workerSpriteManagerTestAdapter.SetAgentBasicData(instance.CurrentData.appearance.spriteSet, instance.CurrentData.appearance);

            agentLayerTestAdapter.GameObject = AgentLayer.currentLayer;
            instance.UpdateAgentModel(agentLayerTestAdapter);
        }

        /// <summary>
        ///     Runs before confirming the Strengthen Employee window to save appearance data. Needs to run before the Confirm method because the Confirm method unloads the CurrentAgent
        ///     from the customizing window, so it would be too late for us to update the agent. This forcefully updates an agent's data because the game wasn't designed to allow you to
        ///     customize existing agents, so the game assumes the agent was already created before this step.
        /// </summary>
        // ReSharper disable InconsistentNaming
        [EntryPoint]
        [ExcludeFromCodeCoverage(Justification = Messages.UnityCodeCoverageJustification)]
        public static void Prefix([NotNull] CustomizingWindow __instance)
        {
            try
            {
                __instance.PatchBeforeConfirm(new AgentLayerTestAdapter(), new WorkerSpriteManagerTestAdapter());
            }
            catch (Exception ex)
            {
                Harmony_Patch.Instance.Logger.WriteException(ex);

                throw;
            }
        }
    }
}
