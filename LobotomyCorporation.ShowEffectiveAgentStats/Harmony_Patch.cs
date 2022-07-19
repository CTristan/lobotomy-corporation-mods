using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Harmony;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Implementations;
using LobotomyCorporationMods.Common.Interfaces;
using UnityEngine;

namespace LobotomyCorporation.ShowEffectiveAgentStats
{
    [SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores")]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public sealed class Harmony_Patch
    {
        private static IFile File;
        private static string LogFile;

        /// <summary>
        ///     Do not use for testing as it causes an exception. Use the other constructor instead.
        /// </summary>
        public Harmony_Patch()
        {
            var dataPath = Application.dataPath + @"/BaseMods/ShowEffectiveAgentStats/";
            Initialize(dataPath);
            InitializeHarmonyPatch();
        }

        /// <summary>
        ///     Entry point for testing.
        /// </summary>
        public Harmony_Patch(string dataPath)
        {
            Initialize(dataPath);
        }

        /// <summary>
        ///     Loads data files.
        /// </summary>
        private static void Initialize(string dataPath)
        {
            File = new File();
            LogFile = dataPath + "ShowEffectiveAgentStats_Log.txt";
        }

        /// <summary>
        ///     Patches all of the relevant method calls through Harmony.
        /// </summary>
        private static void InitializeHarmonyPatch()
        {
            try
            {
                var harmonyInstance = HarmonyInstance.Create("ShowEffectiveAgentStats");
                if (harmonyInstance == null)
                {
                    throw new InvalidOperationException(nameof(harmonyInstance));
                }

                var harmonyMethod = new HarmonyMethod(typeof(Harmony_Patch).GetMethod("SetStatPostfix"));
                harmonyInstance.Patch(typeof(AgentInfoWindow.WorkerPrimaryStatUI).GetMethod("SetStat", AccessTools.all),
                    null, harmonyMethod);
            }
            catch (Exception ex)
            {
                WriteToLog(File, ex.Message + Environment.NewLine + ex.StackTrace);
                throw;
            }
        }

        [SuppressMessage("ReSharper", "StringLiteralTypo")]
        public static void SetStat([NotNull] AgentInfoWindow.WorkerPrimaryStatUI __instance, AgentModel agent)
        {
            var statName = string.Empty;
            var statValue = 0;
            var statType = __instance.type;
            var statEffectList = __instance.list;

            switch (statType)
            {
                case RwbpType.R:
                    {
                        statName = "Rstat";
                        statValue = agent.Rstat;

                        var originalValue = agent.primaryStat.maxHP + agent.titleBonus.maxHP;
                        var additionalValue = agent.maxHp - originalValue;
                        var workExperience = Mathf.FloorToInt(agent.primaryStatExp.hp);
                        SetStatText(statEffectList[0], originalValue, additionalValue, workExperience);

                        break;
                    }
            }

            __instance.StatName.text = string.Format(CultureInfo.CurrentCulture, "{0} {1}",
                LocalizeTextDataModel.instance.GetText(statName), AgentModel.GetLevelGradeText(statValue));
        }

        private static void SetStatText([NotNull] AgentInfoWindow.WorkerPrimaryStatUnit statUnit, int originalValue,
            int additionalValue, int experienceValue)
        {
            if (additionalValue == 0)
            {
                statUnit.StatValue.text = originalValue.ToString(CultureInfo.CurrentCulture);

                if (experienceValue > 0)
                {
                    statUnit.StatValue.text += string.Format(CultureInfo.CurrentCulture, @"+{0}", experienceValue);
                }

                return;
            }

            var effectiveValue = originalValue + additionalValue;
            var combinedValue = effectiveValue + experienceValue;
            string additionalValueColor;
            string operation;

            if (additionalValue > 0)
            {
                additionalValueColor = AgentInfoWindow.currentWindow.Additional_Plus_ValueColor;
                operation = "+";
            }
            else
            {
                additionalValueColor = AgentInfoWindow.currentWindow.Additional_Minus_ValueColor;
                operation = "-";
            }

            var template = @"{0}/{1} ({2}<color=#{3}>{4}{5}</color>+{6})";
            statUnit.StatValue.text = string.Format(template, effectiveValue, combinedValue, originalValue,
                additionalValueColor, operation, Mathf.Abs(additionalValue), experienceValue);
        }

        /// <summary>
        ///     Writes to a log file.
        /// </summary>
        /// <param name="file">The file interface.</param>
        /// <param name="message">The message to log.</param>
        private static void WriteToLog([NotNull] IFile file, [NotNull] string message)
        {
            file.WriteAllText(LogFile, message);
        }
    }
}
