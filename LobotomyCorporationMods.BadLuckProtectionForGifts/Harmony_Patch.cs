using System;
using Harmony;
using JetBrains.Annotations;
using LobotomyCorporationMods.BadLuckProtectionForGifts.Implementations;
using LobotomyCorporationMods.BadLuckProtectionForGifts.Interfaces;
using UnityEngine;

// ReSharper disable CommentTypo
// ReSharper disable IdentifierTypo
// ReSharper disable InconsistentNaming
namespace LobotomyCorporationMods.BadLuckProtectionForGifts
{
    public sealed class Harmony_Patch
    {
        public static AgentWorkTracker AgentWorkTracker;
        public static IFile File;
        private static string LogFile;
        private static string TrackerFile;

        public Harmony_Patch()
        {
            File = new File();
            var dataPath = Application.dataPath + @"/BaseMods/BadLuckProtectionForGifts/";
            TrackerFile = dataPath + "BadLuckProtectionForGifts.dat";
            LogFile = dataPath + "BadLuckProtectionForGifts_Log.txt";
            AgentWorkTracker = AgentWorkTracker.FromString(File.ReadAllText(TrackerFile));
            try
            {
                var harmonyInstance = HarmonyInstance.Create("BadLuckProtectionForGifts");
                var harmonyMethod = new HarmonyMethod(typeof(Harmony_Patch).GetMethod("CallNewgame"));
                harmonyInstance.Patch(typeof(AlterTitleController).GetMethod("CallNewgame", AccessTools.all), null,
                    harmonyMethod);
                harmonyInstance.Patch(typeof(NewTitleScript).GetMethod("ClickAfterNewGame", AccessTools.all), null,
                    harmonyMethod);
                harmonyMethod = new HarmonyMethod(typeof(Harmony_Patch).GetMethod("FinishWorkSuccessfully"));
                harmonyInstance.Patch(typeof(UseSkill).GetMethod("FinishWorkSuccessfully", AccessTools.all),
                    harmonyMethod, null);
                harmonyMethod = new HarmonyMethod(typeof(Harmony_Patch).GetMethod("GetProb"));
                harmonyInstance.Patch(typeof(CreatureEquipmentMakeInfo).GetMethod("GetProb", AccessTools.all), null,
                    harmonyMethod);
                harmonyMethod = new HarmonyMethod(typeof(Harmony_Patch).GetMethod("OnClickNextDay"));
                harmonyInstance.Patch(typeof(GameSceneController).GetMethod("OnClickNextDay", AccessTools.all), null,
                    harmonyMethod);
                harmonyMethod = new HarmonyMethod(typeof(Harmony_Patch).GetMethod("OnStageStart"));
                harmonyInstance.Patch(typeof(GameSceneController).GetMethod("OnStageStart", AccessTools.all), null,
                    harmonyMethod);
            }
            catch (Exception ex)
            {
                WriteToLog(File, ex.Message + Environment.NewLine + ex.StackTrace);
                throw;
            }
        }

        /// <summary>
        ///     Runs after the original CallNewgame method does to reset our agent work when the player starts a new game.
        /// </summary>
        /// <param name="__instance">The AlterTitleController event that indicates we're starting a new game.</param>
        public static void CallNewgame([NotNull] AlterTitleController __instance)
        {
            AgentWorkTracker = new AgentWorkTracker();
            SaveTracker(File);
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
        }

        /// <summary>
        ///     Runs after the original GetProb method finishes to add our own probability bonus.
        /// </summary>
        /// <param name="__instance">The gift being returned from the creature.</param>
        /// <param name="__result">The probability of getting the gift.</param>
        /// <returns>Always returns false so that we skip the original method entirely.</returns>
        public static void GetProb([NotNull] CreatureEquipmentMakeInfo __instance, ref float __result)
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
        }

        /// <summary>
        ///     Runs after the original OnClickNextDay method to save our tracker progress. We only save when going to the next
        ///     day because it doesn't make sense that an agent would remember their creature experience if the day is reset.
        /// </summary>
        /// <param name="__instance">The GameSceneController instance.</param>
        public static void OnClickNextDay([NotNull] GameSceneController __instance)
        {
            SaveTracker(File);
        }

        /// <summary>
        ///     Runs after the original OnStageStart method to reset our tracker progress. We reset the progress on restart
        ///     because it doesn't make sense that an agent would remember their creature experience if the day is reset.
        /// </summary>
        /// <param name="__instance">The GlobalGameManager instance.</param>
        public static void OnStageStart([NotNull] GameSceneController __instance)
        {
            AgentWorkTracker = AgentWorkTracker.FromString(File.ReadAllText(TrackerFile));
        }

        /// <summary>
        ///     Writes the AgentWorkTracker to a text file.
        /// </summary>
        /// <param name="file">The file interface.</param>
        private static void SaveTracker([NotNull] IFile file)
        {
            file.WriteAllText(TrackerFile, AgentWorkTracker.ToString());
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
    }
}
