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
        // ReSharper disable InconsistentNaming
        public static void Postfix([CanBeNull] CreatureEquipmentMakeInfo __instance, ref float __result)
        {
            if (__instance == null)
            {
                return;
            }

            try
            {
                var giftName = __instance.equipTypeInfo?.Name;

                // If creature has no gift then giftName will be null
                if (giftName == null)
                {
                    return;
                }

                var probabilityBonus = Harmony_Patch.Instance.AgentWorkTracker.GetLastAgentWorkCountByGift(giftName) / 100f;
                __result += probabilityBonus;

                // Prevent potential overflow issues
                if (__result > 1f)
                {
                    __result = 1f;
                }
            }
            catch (Exception ex)
            {
                Harmony_Patch.Instance.FileManager.WriteToLog(ex);

                throw;
            }
        }
    }
}
