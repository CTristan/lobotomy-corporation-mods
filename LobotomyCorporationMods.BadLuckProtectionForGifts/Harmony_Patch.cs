using System;
using System.Collections.Generic;
using System.IO;
using Harmony;
using JetBrains.Annotations;
using UnityEngine;

// ReSharper disable InconsistentNaming
namespace LobotomyCorporationMods.BadLuckProtectionForGifts
{
    public sealed class Harmony_Patch
    {
        public static Dictionary<long, float> NumberOfTimesWorkedByAgent;

        public Harmony_Patch()
        {
            NumberOfTimesWorkedByAgent = new Dictionary<long, float>();
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
            var probabilityBonus = 0f;
            if (ResearchDataModel.instance.IsUpgradedAbility("add_efo_gift_prob"))
            {
                probabilityBonus += __instance.prob;
            }

            probabilityBonus += NumberOfTimesWorkedByAgent[1] / 100f;
            __result = __instance.prob + probabilityBonus;

            // Prevent potential overflow issues
            if (__result > 1f)
            {
                __result = 1f;
            }

            return false;
        }
    }
}
