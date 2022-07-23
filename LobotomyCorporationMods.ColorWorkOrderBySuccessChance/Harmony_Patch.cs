using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using CommandWindow;
using Harmony;
using JetBrains.Annotations;
using LobotomyCorporationMods.ColorWorkOrderBySuccessChance.Extensions;
using UnityEngine;

#pragma warning disable CA1707
namespace LobotomyCorporationMods.ColorWorkOrderBySuccessChance
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class Harmony_Patch
    {
        private const string ModFileName = "LobotomyCorporationMods.ColorWorkOrderBySuccessChance.dll";
        private static string DataPath;
        private static string LogFile;

        private static Color originalOrderColor;

        /// <summary>
        ///     Do not use for testing as it causes an exception. Use the other constructor instead.
        /// </summary>
        public Harmony_Patch()
        {
            DataPath = ModExtensions.GetDataPath(ModFileName);

            Initialize();
            InitializeHarmonyPatch();
        }

        /// <summary>
        ///     Entry point for testing.
        /// </summary>
        public Harmony_Patch(string dataPath)
        {
            DataPath = dataPath;

            Initialize();
        }

        /// <summary>
        ///     Loads data files.
        /// </summary>
        private static void Initialize()
        {
            LogFile = Path.Combine(DataPath, "log.txt");
            originalOrderColor = Color.white;
        }

        /// <summary>
        ///     Patches all of the relevant method calls through Harmony.
        /// </summary>
        private static void InitializeHarmonyPatch()
        {
            try
            {
                var harmonyInstance = HarmonyInstance.Create("ColorWorkOrderBySuccessChance");
                if (harmonyInstance == null)
                {
                    throw new InvalidOperationException(nameof(harmonyInstance));
                }

                var harmonyMethod = new HarmonyMethod(typeof(Harmony_Patch).GetMethod("OnOverlayEnter"));
                harmonyInstance.Patch(typeof(AgentSlot).GetMethod("OnOverlayEnter", new Type[] { }), null, harmonyMethod);
            }
            catch (Exception ex)
            {
                ModExtensions.WriteAllText(LogFile, ex.Message + Environment.NewLine + ex.StackTrace);
                throw;
            }
        }

        private static float GetSuccessChance([NotNull] CreatureModel creature, [NotNull] AgentModel agent,
            SkillTypeInfo skill)
        {
            var successChance = Math.Min(creature.GetWorkSuccessProb(agent, skill) * 100f + agent.workProb / 5f, 95f);
            var successChanceReduction = creature.ProbReductionValue > 0
                ? creature.ProbReductionValue
                : creature.GetRedusedWorkProbByCounter();
            successChance -= successChanceReduction;

            return Math.Max(successChance / 100f, 0f);
        }

        private static CreatureFeelingState GetCreatureState([NotNull] CreatureModel creature, float successChance)
        {
            var maxBound = creature.metaInfo.feelingStateCubeBounds.GetLastBound();
            var energyCubeNum = Mathf.FloorToInt(maxBound * successChance);

            return creature.metaInfo.feelingStateCubeBounds.CalculateFeelingState(energyCubeNum);
        }

        public static void OnOverlayEnter([NotNull] AgentSlot __instance)
        {
            var commandWindow = CommandWindow.CommandWindow.CurrentWindow;
            var creature = commandWindow.WorkAllocate.CurrentModel;

            var successChance = GetSuccessChance(creature, __instance.CurrentAgent, commandWindow.CurrentSkill);
            var creatureState = GetCreatureState(creature, successChance);

            CreatureLayer.IsolateRoomUIData.GetFeelingStateData(creatureState, out _, out commandWindow.OrderColor);
        }
    }
}
#pragma warning restore CA1707
