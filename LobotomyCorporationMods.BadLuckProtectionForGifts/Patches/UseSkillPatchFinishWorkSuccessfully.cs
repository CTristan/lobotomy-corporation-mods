// SPDX-License-Identifier: MIT

#region

using System;
using System.Diagnostics.CodeAnalysis;
using Harmony;
using JetBrains.Annotations;
using LobotomyCorporationMods.BadLuckProtectionForGifts.Extensions;
using LobotomyCorporationMods.Common.Extensions;
using LobotomyCorporationMods.Common.Implementations;

#endregion

namespace LobotomyCorporationMods.BadLuckProtectionForGifts.Patches
{
    [HarmonyPatch(typeof(UseSkill), "FinishWorkSuccessfully")]
    [SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores")]
    [SuppressMessage("Style", "IDE1006:Naming Styles")]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public static class UseSkillPatchFinishWorkSuccessfully
    {
        public static void Prefix([NotNull] UseSkill __instance)
        {
            try
            {
                Guard.Against.Null(__instance, nameof(__instance));

                var equipmentMakeInfo = __instance.GetCreatureEquipmentMakeInfo();

                // If the creature has no gift it returns null
                if (equipmentMakeInfo?.equipTypeInfo?.Name is null)
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
                Harmony_Patch.Instance.Logger.WriteToLog(ex);

                throw;
            }
        }
    }
}
