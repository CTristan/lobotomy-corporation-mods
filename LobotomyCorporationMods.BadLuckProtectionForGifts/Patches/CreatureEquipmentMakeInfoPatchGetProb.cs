// SPDX-License-Identifier: MIT

using System;
using System.Diagnostics.CodeAnalysis;
using Harmony;
using JetBrains.Annotations;

namespace LobotomyCorporationMods.BadLuckProtectionForGifts.Patches
{
    [HarmonyPatch(typeof(CreatureEquipmentMakeInfo), "GetProb")]
    [SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores")]
    public static class CreatureEquipmentMakeInfoPatchGetProb
    {
        public static void Postfix([NotNull] CreatureEquipmentMakeInfo __instance, ref float __result)
        {
            try
            {
                var giftName = __instance.equipTypeInfo?.Name;

                // If creature has no gift then giftName will be null
                if (giftName == null)
                {
                    return;
                }

                var probabilityBonus = Harmony_Patch.GetAgentWorkTracker().GetLastAgentWorkCountByGift(giftName) / 100f;
                __result += probabilityBonus;

                // Prevent potential overflow issues
                if (__result > 1f)
                {
                    __result = 1f;
                }
            }
            catch (Exception ex)
            {
                Harmony_Patch.GetFileManager().WriteToLog(ex);

                throw;
            }
        }
    }
}
