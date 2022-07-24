using System;
using System.Diagnostics.CodeAnalysis;
using Harmony;
using JetBrains.Annotations;
using LobotomyCorporationMods.BadLuckProtectionForGifts.Implementations;
using LobotomyCorporationMods.BadLuckProtectionForGifts.Interfaces;
using LobotomyCorporationMods.Common.Implementations;
using LobotomyCorporationMods.Common.Interfaces;

// ReSharper disable CommentTypo
// ReSharper disable IdentifierTypo
// ReSharper disable InconsistentNaming
namespace LobotomyCorporationMods.BadLuckProtectionForGifts
{
    [SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores")]
    public sealed class Harmony_Patch
    {
        private const string ModFileName = "LobotomyCorporationMods.BadLuckProtectionForGifts.dll";

        [NotNull] private static IAgentWorkTracker AgentWorkTracker = new AgentWorkTracker();
        private static IFileManager FileManager;
        private static string TrackerFile;

        /// <summary>
        ///     Do not use for testing as it causes an exception. Use the other constructor instead.
        /// </summary>
        public Harmony_Patch()
        {
            FileManager = new FileManager(ModFileName);
            Initialize();
            InitializeHarmonyPatch();
        }

        /// <summary>
        ///     Entry point for testing.
        /// </summary>
        public Harmony_Patch(IFileManager fileManager)
        {
            FileManager = fileManager;
            Initialize();
        }

        /// <summary>
        ///     Loads data files.
        /// </summary>
        private static void Initialize()
        {
            TrackerFile = FileManager.GetFile("BadLuckProtectionForGifts.dat");
            AgentWorkTracker = AgentWorkTracker.FromString(FileManager.ReadAllText(TrackerFile, true));
        }

        /// <summary>
        ///     Patches all of the relevant method calls through Harmony.
        /// </summary>
        private static void InitializeHarmonyPatch()
        {
            try
            {
                var harmonyInstance = HarmonyInstance.Create("BadLuckProtectionForGifts");
                if (harmonyInstance == null)
                {
                    throw new InvalidOperationException(nameof(harmonyInstance));
                }

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
                var message = ex.Message + Environment.NewLine + ex.StackTrace;
                FileManager.WriteToLog(message);
                throw;
            }
        }

        [NotNull]
        public static IAgentWorkTracker GetAgentWorkTracker()
        {
            return AgentWorkTracker;
        }

        /// <summary>
        ///     Runs after the original CallNewgame method does to reset our agent work when the player starts a new game.
        /// </summary>
        /// <param name="__instance">The AlterTitleController event that indicates we're starting a new game.</param>
        // ReSharper disable once UnusedParameter.Global
        public static void CallNewgame([NotNull] AlterTitleController __instance)
        {
            AgentWorkTracker = new AgentWorkTracker();
            SaveTracker();
        }

        /// <summary>
        ///     Runs before the original FinishWorkSuccessfully method does to increment the number of times the agent
        ///     worked on the creature.
        /// </summary>
        /// <param name="__instance">The UseSkill event that includes the agent data.</param>
        public static void FinishWorkSuccessfully([NotNull] UseSkill __instance)
        {
            var equipmentMakeInfo = __instance.targetCreature?.metaInfo?.equipMakeInfos?.Find(x =>
                x?.equipTypeInfo?.type == EquipmentTypeInfo.EquipmentType.SPECIAL);

            // If the creature has no gift it returns null
            if (equipmentMakeInfo?.equipTypeInfo?.Name == null)
            {
                return;
            }

            var giftName = equipmentMakeInfo.equipTypeInfo.Name;
            var agentId = __instance.agent?.instanceId ?? 0;
            var numberOfSuccesses = __instance.successCount;
            AgentWorkTracker.IncrementAgentWorkCount(giftName, agentId, numberOfSuccesses);
        }

        /// <summary>
        ///     Runs after the original GetProb method finishes to add our own probability bonus.
        /// </summary>
        /// <param name="__instance">The gift being returned from the creature.</param>
        /// <param name="__result">The probability of getting the gift.</param>
        /// <returns>Always returns false so that we skip the original method entirely.</returns>
        public static void GetProb([NotNull] CreatureEquipmentMakeInfo __instance, ref float __result)
        {
            var giftName = __instance.equipTypeInfo?.Name;

            // If creature has no gift then giftName will be null
            if (giftName == null)
            {
                return;
            }

            var probabilityBonus = AgentWorkTracker.GetLastAgentWorkCountByGift(giftName) / 100f;
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
        // ReSharper disable once UnusedMember.Global
        // ReSharper disable once UnusedParameter.Global
        public static void OnClickNextDay([NotNull] GameSceneController __instance)
        {
            SaveTracker();
        }

        /// <summary>
        ///     Runs after the original OnStageStart method to reset our tracker progress. We reset the progress on restart
        ///     because it doesn't make sense that an agent would remember their creature experience if the day is reset.
        /// </summary>
        /// <param name="__instance">The GlobalGameManager instance.</param>
        // ReSharper disable once UnusedMember.Global
        // ReSharper disable once UnusedParameter.Global
        public static void OnStageStart([NotNull] GameSceneController __instance)
        {
            AgentWorkTracker = AgentWorkTracker.FromString(FileManager.ReadAllText(TrackerFile));
        }

        /// <summary>
        ///     Writes the AgentWorkTracker to a text file.
        /// </summary>
        private static void SaveTracker()
        {
            FileManager.WriteAllText(TrackerFile, AgentWorkTracker.ToString());
        }
    }
}
