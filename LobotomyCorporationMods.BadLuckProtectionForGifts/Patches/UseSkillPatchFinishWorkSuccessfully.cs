// SPDX-License-Identifier: MIT

#region

using System;
using System.Diagnostics.CodeAnalysis;
using Harmony;
using LobotomyCorporationMods.BadLuckProtectionForGifts.Extensions;
using LobotomyCorporationMods.BadLuckProtectionForGifts.Interfaces;
using LobotomyCorporationMods.Common.Attributes;

#endregion

namespace LobotomyCorporationMods.BadLuckProtectionForGifts.Patches
{
    [HarmonyPatch(typeof(UseSkill), "FinishWorkSuccessfully")]
    public static class UseSkillPatchFinishWorkSuccessfully
    {
        public static void PatchAfterFinishWorkSuccessfully(this UseSkill instance, IAgentWorkTracker agentWorkTracker)
        {
            if (instance is null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

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

        // ReSharper disable once InconsistentNaming
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
    }
}
