// SPDX-License-Identifier: MIT

using System;
using System.Diagnostics.CodeAnalysis;
using Harmony;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Implementations;

namespace LobotomyCorporationMods.BadLuckProtectionForGifts.Patches
{
    [HarmonyPatch(typeof(CreatureEquipmentMakeInfo), "GetProb")]
    [SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores")]
    public static class CreatureEquipmentMakeInfoPatchGetProb
    {
        // ReSharper disable InconsistentNaming
        public static void Postfix([NotNull] CreatureEquipmentMakeInfo __instance, ref float __result)
        {
            try
            {
                Guard.Against.Null(__instance, nameof(__instance));

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
    }
}
