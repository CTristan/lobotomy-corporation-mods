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
        public static void PatchAfterConfirm(this CustomizingWindow instance, IAgentLayerAdapter agentLayerAdapter, IWorkerSpriteManagerAdapter workerSpriteManagerAdapter)
        {
            if (instance is null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            instance.SaveAgentAppearance();
            instance.RenameAgent();
            instance.CurrentData.appearance.SetResrouceData();

            workerSpriteManagerAdapter.GameObject = WorkerSpriteManager.instance;
            workerSpriteManagerAdapter.SetAgentBasicData(instance.CurrentData.appearance.spriteSet, instance.CurrentData.appearance);

            agentLayerAdapter.GameObject = AgentLayer.currentLayer;
            instance.UpdateAgentModel(agentLayerAdapter);
        }

        /// <summary>
        ///     Runs after confirming the Strengthen Employee window to save appearance data.
        /// </summary>
        // ReSharper disable InconsistentNaming
        [EntryPoint]
        [ExcludeFromCodeCoverage]
        public static void Postfix(CustomizingWindow __instance)
        {
            try
            {
                __instance.PatchAfterConfirm(new AgentLayerAdapter(), new WorkerSpriteManagerAdapter());
            }
            catch (Exception ex)
            {
                Harmony_Patch.Instance.Logger.WriteToLog(ex);

                throw;
            }
        }
    }
}
