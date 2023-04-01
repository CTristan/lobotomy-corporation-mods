// SPDX-License-Identifier: MIT

#region

using System;
using System.Diagnostics.CodeAnalysis;
using Customizing;
using Harmony;
using LobotomyCorporationMods.Common.Attributes;
using LobotomyCorporationMods.Common.Implementations.Adapters;
using LobotomyCorporationMods.Common.Interfaces.Adapters;
using LobotomyCorporationMods.FreeCustomization.Extensions;

#endregion

namespace LobotomyCorporationMods.FreeCustomization.Patches
{
    [HarmonyPatch(typeof(CustomizingWindow), "Confirm")]
    public static class CustomizingWindowPatchConfirm
    {
        public static void PatchBeforeConfirm(this CustomizingWindow instance, IAgentLayerAdapter agentLayerAdapter, IWorkerSpriteManagerAdapter workerSpriteManagerAdapter)
        {
            // We only want to save the appearance data if this is an existing agent, otherwise we'll let the game generate it as usual
            if (instance.CurrentWindowType != CustomizingType.GENERATE)
            {
                instance.SaveAgentAppearance();
                instance.RenameAgent();
                instance.CurrentData.appearance.SetResrouceData();

                workerSpriteManagerAdapter.GameObject = WorkerSpriteManager.instance;
                workerSpriteManagerAdapter.SetAgentBasicData(instance.CurrentData.appearance.spriteSet, instance.CurrentData.appearance);

                agentLayerAdapter.GameObject = AgentLayer.currentLayer;
                instance.UpdateAgentModel(agentLayerAdapter);
            }
        }

        /// <summary>
        ///     Runs after confirming the Strengthen Employee window to save appearance data.
        /// </summary>
        // ReSharper disable InconsistentNaming
        [EntryPoint]
        [ExcludeFromCodeCoverage]
        public static void Prefix(CustomizingWindow __instance)
        {
            try
            {
                if (__instance is null)
                {
                    throw new ArgumentNullException(nameof(__instance));
                }

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
