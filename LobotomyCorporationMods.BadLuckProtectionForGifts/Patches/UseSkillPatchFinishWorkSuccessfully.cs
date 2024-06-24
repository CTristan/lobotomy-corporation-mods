// SPDX-License-Identifier: MIT

#region

using System;
using System.Diagnostics.CodeAnalysis;
using Harmony;
using LobotomyCorporationMods.BadLuckProtectionForGifts.Extensions;
using LobotomyCorporationMods.BadLuckProtectionForGifts.Interfaces;
using LobotomyCorporationMods.Common.Attributes;
using LobotomyCorporationMods.Common.Extensions;
using LobotomyCorporationMods.Common.Implementations;

#endregion

namespace LobotomyCorporationMods.BadLuckProtectionForGifts.Patches
{
    [HarmonyPatch(typeof(UseSkill), "FinishWorkSuccessfully")]
    public static class UseSkillPatchFinishWorkSuccessfully
    {
        public static void PatchAfterFinishWorkSuccessfully(this UseSkill instance, IAgentWorkTracker agentWorkTracker)
        {
            Guard.Against.Null(instance, nameof(instance));
            Guard.Against.Null(agentWorkTracker, nameof(agentWorkTracker));

            var equipmentMakeInfo = instance.GetCreatureEquipmentMakeInfo();

            // If the creature has no gift it returns null
            if (equipmentMakeInfo is null)
            {
                return;
            }

            var giftName = equipmentMakeInfo.equipTypeInfo.Name;
            var agentId = instance.agent.instanceId;
            var numberOfSuccesses = instance.successCount;

            agentWorkTracker.IncrementAgentWorkCount(giftName, agentId, numberOfSuccesses);
        }

        /// <summary>
        ///     Runs after an agent finishes working with an abnormality to increment their work count.
        /// </summary>
        /// <param name="__instance"></param>
        // ReSharper disable InconsistentNaming
        [EntryPoint]
        [ExcludeFromCodeCoverage]
        public static void Postfix(UseSkill __instance)
        {
            try
            {
                __instance.PatchAfterFinishWorkSuccessfully(Harmony_Patch.Instance.AgentWorkTracker);
            }
            catch (Exception ex)
            {
                Harmony_Patch.Instance.Logger.WriteToLog(ex);

                throw;
            }
        }
        // ReSharper enable InconsistentNaming
    }
}
