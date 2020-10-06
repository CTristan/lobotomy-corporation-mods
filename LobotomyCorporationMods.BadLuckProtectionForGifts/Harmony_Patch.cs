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
                harmonyInstance.Patch(typeof(CreatureEquipmentMakeInfo).GetMethod("GetProb", AccessTools.all), null,
                    new HarmonyMethod(typeof(Harmony_Patch).GetMethod("GetProb")));
            }
            catch (Exception ex)
            {
                File.WriteAllText(Application.dataPath + "/BaseMods/BadLuckProtectionForGifts_Log.txt",
                    ex.Message + Environment.NewLine + ex.StackTrace);
            }
        }

        /// <summary>
        ///     Runs after the original GetProb method finishes to add our own probability bonus.
        /// </summary>
        /// <param name="__instance">The gift being returned from the creature.</param>
        /// <param name="__result">The probability of getting the gift.</param>
        /// <returns>Always returns false so that we skip the original method entirely.</returns>
        public static bool GetProb([NotNull] CreatureEquipmentMakeInfo __instance, ref float __result)
        {
            var probabilityBonus = NumberOfTimesWorkedByAgent[1] / 100f;
            __result += probabilityBonus;

            // Prevent potential overflow issues
            if (__result > 1f)
            {
                __result = 1f;
            }

            return false;
        }
    }
}
