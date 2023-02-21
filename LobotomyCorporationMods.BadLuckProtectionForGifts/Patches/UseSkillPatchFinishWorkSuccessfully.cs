// SPDX-License-Identifier: MIT

#region

using System;
using Harmony;
using LobotomyCorporationMods.BadLuckProtectionForGifts.Extensions;

#endregion

namespace LobotomyCorporationMods.BadLuckProtectionForGifts.Patches
{
    [HarmonyPatch(typeof(UseSkill), "FinishWorkSuccessfully")]
    public static class UseSkillPatchFinishWorkSuccessfully
    {
        // ReSharper disable once InconsistentNaming
        public static void Prefix(UseSkill? __instance)
        {
            try
            {
                if (__instance is null)
                {
                    throw new ArgumentNullException(nameof(__instance));
                }

                var equipmentMakeInfo = __instance.GetCreatureEquipmentMakeInfo();

                // If the creature has no gift it returns null
                if (equipmentMakeInfo is null)
                {
                    return;
                }

                var giftName = equipmentMakeInfo.equipTypeInfo.Name;
                var agentId = __instance.agent.instanceId;
                var numberOfSuccesses = __instance.successCount;

                Harmony_Patch.Instance.AgentWorkTracker.IncrementAgentWorkCount(giftName, agentId, numberOfSuccesses);
            }
            catch (Exception ex)
            {
                Harmony_Patch.Instance.Logger.WriteToLog(ex);

                throw;
            }
        }
    }
}
