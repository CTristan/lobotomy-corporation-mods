using System;
using System.Diagnostics.CodeAnalysis;
using Harmony;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Implementations;
using LobotomyCorporationMods.Common.Interfaces;
using UnityEngine;

namespace LobotomyCorporationMods.StartNewGameWithNoAgents
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores")]
    public class Harmony_Patch
    {
        private static IFile File;
        private static string LogFile;

        /// <summary>
        ///     Do not use for testing as it causes an exception. Use the other constructor instead.
        /// </summary>
        public Harmony_Patch()
        {
            var dataPath = Application.dataPath + @"/BaseMods/StartNewGameWithNoAgents/";
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
            LogFile = dataPath + "StartNewGameWithNoAgents_Log.txt";
        }

        /// <summary>
        ///     Patches all of the relevant method calls through Harmony.
        /// </summary>
        private static void InitializeHarmonyPatch()
        {
            try
            {
                var harmonyInstance = HarmonyInstance.Create("StartNewGameWithNoAgents");
                if (harmonyInstance == null)
                {
                    throw new InvalidOperationException(nameof(harmonyInstance));
                }

                var harmonyMethod = new HarmonyMethod(typeof(Harmony_Patch).GetMethod("InitStoryModePostfix"));
                harmonyInstance.Patch(typeof(GlobalGameManager).GetMethod("InitStoryMode", AccessTools.all), null,
                    harmonyMethod);
            }
            catch (Exception ex)
            {
                WriteToLog(File, ex.Message + Environment.NewLine + ex.StackTrace);
                throw;
            }
        }

        public static void InitStoryModePostfix(GlobalGameManager __instance)
        {
            const int agentCreationCost = 2;

            AgentManager.instance.Init();

            // Give an additional 2 LOB points to pay for a new agent
            MoneyModel.instance.Add(agentCreationCost);
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
