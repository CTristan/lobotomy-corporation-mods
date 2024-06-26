// SPDX-License-Identifier: MIT

#region

using System;
using System.Diagnostics.CodeAnalysis;
using Customizing;
using Harmony;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Attributes;
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
        public static void PatchBeforeConfirm([NotNull] this CustomizingWindow instance, [NotNull] IAgentLayerAdapter agentLayerAdapter,
            [NotNull] IWorkerSpriteManagerAdapter workerSpriteManagerAdapter)
        {
            Guard.Against.Null(instance, nameof(instance));
            Guard.Against.Null(agentLayerAdapter, nameof(agentLayerAdapter));
            Guard.Against.Null(workerSpriteManagerAdapter, nameof(workerSpriteManagerAdapter));

            // The original method does this and other things for newly-generated agents, so we only want to do this
            // when we're updating existing agents.
            if (instance.CurrentWindowType == CustomizingType.GENERATE)
            {
                return;
            }

            instance.SaveAgentAppearance();
            instance.RenameAgent();
            instance.CurrentData.appearance.SetResrouceData();

            workerSpriteManagerAdapter.GameObject = WorkerSpriteManager.instance;
            workerSpriteManagerAdapter.SetAgentBasicData(instance.CurrentData.appearance.spriteSet,
                instance.CurrentData.appearance);

            agentLayerAdapter.GameObject = AgentLayer.currentLayer;
            instance.UpdateAgentModel(agentLayerAdapter);
        }

        /// <summary>
        ///     Runs before confirming the Strengthen Employee window to save appearance data.
        ///
        ///     Needs to run before the Confirm method because the Confirm method unloads the CurrentAgent from the
        ///     customizing window, so it would be too late for us to update the agent.
        ///
        ///     This forcefully updates an agent's data because the game wasn't designed to allow you to
        ///     customize existing agents, so the game assumes the agent was already created before this step.
        /// </summary>
        // ReSharper disable InconsistentNaming
        [EntryPoint]
        [ExcludeFromCodeCoverage]
        public static void Prefix([NotNull] CustomizingWindow __instance)
        {
            try
            {
                __instance.PatchBeforeConfirm(new AgentLayerAdapter(), new WorkerSpriteManagerAdapter());
            }
            catch (Exception ex)
            {
                Harmony_Patch.Instance.Logger.WriteException(ex);

                throw;
            }
        }
    }
}
