// SPDX-License-Identifier: MIT

#region

using System;
using Customizing;
using Harmony;
using LobotomyCorporationMods.Common.Implementations.Adapters;
using LobotomyCorporationMods.Common.Interfaces.Adapters;
using LobotomyCorporationMods.FreeCustomization.Extensions;

#endregion

namespace LobotomyCorporationMods.FreeCustomization.Patches
{
    [HarmonyPatch(typeof(CustomizingWindow), "Confirm")]
    public static class CustomizingWindowPatchConfirm
    {
        public static IAgentLayerAdapter LayerAdapter { get; set; } = new AgentLayerAdapter();
        public static IWorkerSpriteManagerAdapter SpriteManagerAdapter { get; set; } = new WorkerSpriteManagerAdapter();

        /// <summary>
        ///     Runs before confirming the Strengthen Employee window to save appearance data.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public static void Prefix(CustomizingWindow? __instance)
        {
            try
            {
                if (__instance is null)
                {
                    throw new ArgumentNullException(nameof(__instance));
                }

                if (__instance.CurrentWindowType == CustomizingType.GENERATE)
                {
                    return;
                }

                __instance.SaveAgentAppearance();
                __instance.RenameAgent();
                __instance.CurrentData.appearance.SetResrouceData();

                SpriteManagerAdapter.GameObject = WorkerSpriteManager.instance;
                SpriteManagerAdapter.SetAgentBasicData(__instance.CurrentData.appearance.spriteSet, __instance.CurrentData.appearance);

                LayerAdapter.GameObject = AgentLayer.currentLayer;
                __instance.UpdateAgentModel(LayerAdapter);
            }
            catch (Exception ex)
            {
                Harmony_Patch.Instance.Logger.WriteToLog(ex);

                throw;
            }
        }
    }
}
