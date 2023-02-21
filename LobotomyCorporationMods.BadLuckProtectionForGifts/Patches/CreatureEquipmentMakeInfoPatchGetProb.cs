// SPDX-License-Identifier: MIT

#region

using System;
using System.Diagnostics.CodeAnalysis;
using Harmony;

#endregion

namespace LobotomyCorporationMods.BadLuckProtectionForGifts.Patches
{
    [HarmonyPatch(typeof(CreatureEquipmentMakeInfo), "GetProb")]
    [SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores")]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("Style", "IDE1006:Naming Styles")]
    public static class CreatureEquipmentMakeInfoPatchGetProb
    {
        public static void Postfix(CreatureEquipmentMakeInfo? __instance, ref float __result)
        {
            try
            {
                if (__instance is null)
                {
                    throw new ArgumentNullException(nameof(__instance));
                }

                var giftName = __instance.equipTypeInfo?.Name;

                // If creature has no gift then giftName will be null
                if (giftName is null)
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
                Harmony_Patch.Instance.Logger.WriteToLog(ex);

                throw;
            }
        }
    }
}
