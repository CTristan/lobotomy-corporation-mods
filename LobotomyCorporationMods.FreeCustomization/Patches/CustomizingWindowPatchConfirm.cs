// SPDX-License-Identifier: MIT

using System;
using System.Diagnostics.CodeAnalysis;
using Customizing;
using Harmony;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Extensions;
using LobotomyCorporationMods.Common.Implementations;
using LobotomyCorporationMods.FreeCustomization.Extensions;

namespace LobotomyCorporationMods.FreeCustomization.Patches
{
    [HarmonyPatch(typeof(CustomizingWindow), "Confirm")]
    public static class CustomizingWindowPatchConfirm
    {
        /// <summary>
        ///     Runs before confirming the Strengthen Employee window to save appearance data.
        /// </summary>
        [SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores")]
        [SuppressMessage("Style", "IDE1006:Naming Styles")]
        // ReSharper disable once InconsistentNaming
        public static void Prefix([NotNull] CustomizingWindow __instance)
        {
            try
            {
                Guard.Against.Null(__instance, nameof(__instance));

                if (__instance.CurrentWindowType == CustomizingType.GENERATE)
                {
                    return;
                }

                __instance.SaveAgentAppearance();

                __instance.CurrentData.appearance.SetResrouceData();
                WorkerSpriteManager.instance.SetAgentBasicData(__instance.CurrentData.appearance.spriteSet, __instance.CurrentData.appearance);

                var agentModel = __instance.CurrentAgent;
                var agentUnit = AgentLayer.currentLayer.GetAgent(agentModel.instanceId);

                if (agentUnit == null)
                {
                    return;
                }

                AgentLayer.currentLayer.RemoveAgent(agentModel);
                AgentLayer.currentLayer.AddAgent(agentModel);
            }
            catch (Exception ex)
            {
                Harmony_Patch.Instance.FileManager.WriteToLog(ex);

                throw;
            }
        }
    }
}
