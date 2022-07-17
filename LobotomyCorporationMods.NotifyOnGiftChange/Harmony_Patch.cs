using System;
using System.Diagnostics.CodeAnalysis;
using Harmony;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Implementations;
using LobotomyCorporationMods.Common.Interfaces;
using UnityEngine;

namespace LobotomyCorporationMods.NotifyOnGiftChange
{
    [SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores")]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class Harmony_Patch
    {
        private static IFile File;
        private static string LogFile;

        /// <summary>
        ///     Do not use for testing as it causes an exception. Use the other constructor instead.
        /// </summary>
        public Harmony_Patch()
        {
            var dataPath = Application.dataPath + @"/BaseMods/BadLuckProtectionForGifts/";
            Initialize(dataPath);
            InitializeHarmonyPatch();
        }

        /// <summary>
        ///     Loads data files.
        /// </summary>
        private static void Initialize(string dataPath)
        {
            File = new File();
            LogFile = dataPath + "BadLuckProtectionForGifts_Log.txt";
        }

        private static void InitializeHarmonyPatch()
        {
            try
            {
                var harmonyInstance = HarmonyInstance.Create("BadLuckProtectionForGifts");
                if (harmonyInstance == null)
                {
                    throw new InvalidOperationException(nameof(harmonyInstance));
                }

                // TODO: CommandWindow
            }
            catch (Exception ex)
            {
                WriteToLog(File, ex.Message + Environment.NewLine + ex.StackTrace);
                throw;
            }
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
