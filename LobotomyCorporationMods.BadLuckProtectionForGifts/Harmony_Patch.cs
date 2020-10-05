using System;
using System.IO;
using Harmony;
using JetBrains.Annotations;
using UnityEngine;

// ReSharper disable InconsistentNaming
namespace LobotomyCorporationMods.BadLuckProtectionForGifts
{
    public class Harmony_Patch
    {
        public Harmony_Patch()
        {
            try
            {
                var harmonyInstance = HarmonyInstance.Create("BadLuckProtectionForGifts");
                harmonyInstance.Patch(typeof(CreatureEquipmentMakeInfo).GetMethod("GetProb", AccessTools.all),
                    new HarmonyMethod(typeof(Harmony_Patch).GetMethod("GetProb")), null);
            }
            catch (Exception ex)
            {
                File.WriteAllText(Application.dataPath + "/BaseMods/BadLuckProtectionForGifts_Log.txt",
                    ex.Message + Environment.NewLine + ex.StackTrace);
            }
        }

        /// <summary>
        ///     Overwrites GetProb with our own logic and result. Original method does not have any side effects.
        /// </summary>
        /// <param name="__instance">The gift being returned from the creature.</param>
        /// <param name="__result">The probability of getting the gift.</param>
        /// <returns>Always returns false so that we skip the original method entirely.</returns>
        public static bool GetProb([NotNull] CreatureEquipmentMakeInfo __instance, out float __result)
        {
            __result = ResearchDataModel.instance.IsUpgradedAbility("add_efo_gift_prob")
                ? __instance.prob + __instance.prob
                : __instance.prob;
            return false;
        }
    }
}
