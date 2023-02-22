// SPDX-License-Identifier: MIT

#region

using System;
using System.Diagnostics.CodeAnalysis;
using Harmony;
using LobotomyCorporationMods.Common.Attributes;

#endregion

namespace LobotomyCorporationMods.BadLuckProtectionForGifts.Patches
{
    [HarmonyPatch(typeof(CreatureEquipmentMakeInfo), "GetProb")]
    public static class CreatureEquipmentMakeInfoPatchGetProb
    {
        public static float PatchedGetProb(this CreatureEquipmentMakeInfo creatureEquipmentMakeInfo, float probability)
        {
            if (creatureEquipmentMakeInfo is null)
            {
                throw new ArgumentNullException(nameof(creatureEquipmentMakeInfo));
            }

            var giftName = creatureEquipmentMakeInfo.equipTypeInfo?.Name;

            // If creature has no gift then giftName will be null
            if (giftName is not null)
            {
                var probabilityBonus = Harmony_Patch.Instance.AgentWorkTracker.GetLastAgentWorkCountByGift(giftName) / 100f;
                probability += probabilityBonus;

                // Prevent potential overflow issues
                if (probability > 1f)
                {
                    probability = 1f;
                }
            }

            return probability;
        }

        [ExcludeFromCodeCoverage]
        [EntryPoint]
        public static void Postfix(CreatureEquipmentMakeInfo? __instance, ref float __result)
        {
            try
            {
                if (__instance is null)
                {
                    throw new ArgumentNullException(nameof(__instance));
                }

                __result = __instance.PatchedGetProb(__result);
            }
            catch (Exception ex)
            {
                Harmony_Patch.Instance.Logger.WriteToLog(ex);

                throw;
            }
        }
    }
}
