using System;
using Harmony;
using JetBrains.Annotations;
using LobotomyCorporationMods.BadLuckProtectionForGifts.Implementations;
using LobotomyCorporationMods.BadLuckProtectionForGifts.Interfaces;
using UnityEngine;

// ReSharper disable InconsistentNaming
namespace LobotomyCorporationMods.BadLuckProtectionForGifts
{
    public sealed class Harmony_Patch
    {
        public static AgentWorkTracker AgentWorkTracker;
        public static IFile File;
        private static string JsonFile;
        private static string LogFile;

        public Harmony_Patch()
        {
            File = new File();
            var dataPath = Application.dataPath + @"/BaseMods/";
            JsonFile = dataPath + "BadLuckProtectionForGifts.json";
            LogFile = dataPath + "BadLuckProtectionForGifts_Log.txt";
            AgentWorkTracker = File.ReadFromJson(JsonFile);
            try
            {
                var harmonyInstance = HarmonyInstance.Create("BadLuckProtectionForGifts");
                var harmonyMethod = new HarmonyMethod(typeof(Harmony_Patch).GetMethod("GetProb"));
                harmonyInstance.Patch(typeof(CreatureEquipmentMakeInfo).GetMethod("GetProb", AccessTools.all), null,
                    harmonyMethod);
                harmonyMethod = new HarmonyMethod(typeof(Harmony_Patch).GetMethod("FinishWorkSuccessfully"));
                harmonyInstance.Patch(typeof(UseSkill).GetMethod("FinishWorkSuccessfully", AccessTools.all),
                    harmonyMethod, null);
            }
            catch (Exception ex)
            {
                WriteToLog(File, ex.Message + Environment.NewLine + ex.StackTrace);
                throw;
            }
        }

        /// <summary>
        ///     Runs before the original FinishWorkSuccessfully method does to increment the number of times the agent
        ///     worked on the creature.
        /// </summary>
        /// <param name="__instance">The UseSkill event that includes the agent data.</param>
        public static void FinishWorkSuccessfully([NotNull] UseSkill __instance)
        {
            var equipmentMakeInfo = __instance.targetCreature.metaInfo.equipMakeInfos.Find(x =>
                x.equipTypeInfo.type == EquipmentTypeInfo.EquipmentType.SPECIAL);
            var giftName = equipmentMakeInfo.equipTypeInfo.Name;
            var agentId = __instance.agent.instanceId;
            AgentWorkTracker.IncrementAgentWorkCount(giftName, agentId);
            WriteToJson(File);
        }

        /// <summary>
        ///     Writes the AgentWorkTracker to a json file.
        /// </summary>
        /// <param name="file">The file interface.</param>
        private static void WriteToJson([NotNull] IFile file)
        {
            file.WriteToJson(JsonFile, AgentWorkTracker);
        }

        /// <summary>
        ///     Writes to a log file.
        /// </summary>
        /// <param name="file">The file interface.</param>
        /// <param name="message">The message to log.</param>
        private static void WriteToLog([NotNull] IFile file, string message)
        {
            file.WriteAllText(LogFile, message);
        }

        /// <summary>
        ///     Runs after the original GetProb method finishes to add our own probability bonus.
        /// </summary>
        /// <param name="__instance">The gift being returned from the creature.</param>
        /// <param name="__result">The probability of getting the gift.</param>
        /// <returns>Always returns false so that we skip the original method entirely.</returns>
        public static bool GetProb([NotNull] CreatureEquipmentMakeInfo __instance, ref float __result)
        {
            var giftName = __instance.equipTypeInfo.Name;
            const long agentId = 1;
            var probabilityBonus = AgentWorkTracker.GetAgentWorkCount(giftName, agentId) / 100f;
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
