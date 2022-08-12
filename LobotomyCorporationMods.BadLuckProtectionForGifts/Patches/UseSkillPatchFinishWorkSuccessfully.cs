using System;
using System.Diagnostics.CodeAnalysis;
using Harmony;
using JetBrains.Annotations;

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
                var equipmentMakeInfo = __instance.targetCreature?.metaInfo?.equipMakeInfos?.Find(x => x?.equipTypeInfo?.type == EquipmentTypeInfo.EquipmentType.SPECIAL);

                // If the creature has no gift it returns null
                if (equipmentMakeInfo?.equipTypeInfo?.Name == null)
                {
                    return;
                }

                var giftName = equipmentMakeInfo.equipTypeInfo.Name;
                var agentId = __instance.agent?.instanceId ?? 0;
                var numberOfSuccesses = __instance.successCount;

                Harmony_Patch.GetAgentWorkTracker().IncrementAgentWorkCount(giftName, agentId, numberOfSuccesses);
            }
            catch (Exception ex)
            {
                Harmony_Patch.GetFileManager().WriteToLog(ex);

                throw;
            }
        }
    }
}
