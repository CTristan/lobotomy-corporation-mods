// SPDX-License-Identifier: MIT

using System;
using System.Diagnostics.CodeAnalysis;
using Harmony;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Implementations;

namespace LobotomyCorporationMods.BadLuckProtectionForGifts.Patches
{
    [HarmonyPatch(typeof(UseSkill), "FinishWorkSuccessfully")]
    [SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores")]
    public static class UseSkillPatchFinishWorkSuccessfully
    {
        // ReSharper disable once InconsistentNaming
        public static void Prefix([NotNull] UseSkill __instance)
        {
            try
            {
                Guard.Against.Null(__instance, nameof(__instance));

                var equipmentMakeInfo = GetCreatureEquipmentMakeInfo(__instance);

                // If the creature has no gift it returns null
                if (equipmentMakeInfo?.equipTypeInfo?.Name == null)
                {
                    return;
                }

                var giftName = equipmentMakeInfo.equipTypeInfo.Name;
                var agentId = __instance.agent?.instanceId ?? 0;
                var numberOfSuccesses = __instance.successCount;

                Harmony_Patch.Instance.AgentWorkTracker.IncrementAgentWorkCount(giftName, agentId, numberOfSuccesses);
            }
            catch (Exception ex)
            {
                // Null argument exception only comes up during testing due to Unity operator overloading.
                // https://github.com/JetBrains/resharper-unity/wiki/Possible-unintended-bypass-of-lifetime-check-of-underlying-Unity-engine-object
                if (ex is ArgumentNullException)
                {
                    return;
                }

                Harmony_Patch.Instance.FileManager.WriteToLog(ex);

                throw;
            }
        }

        [CanBeNull]
        private static CreatureEquipmentMakeInfo GetCreatureEquipmentMakeInfo([NotNull] UseSkill instance)
        {
            var equipmentMakeInfo = instance.targetCreature?.metaInfo?.equipMakeInfos?.Find(x => x?.equipTypeInfo?.type == EquipmentTypeInfo.EquipmentType.SPECIAL);

            return equipmentMakeInfo;
        }
    }
}
