// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Harmony;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Extensions;
using LobotomyCorporationMods.Common.Implementations.Adapters;
using LobotomyCorporationMods.Common.Implementations.LoggerTargets;
using LobotomyCorporationMods.Common.Interfaces;
using LobotomyCorporationMods.Common.Interfaces.Adapters;

namespace LobotomyCorporationMods.Common.Implementations
{
    public class HarmonyPatchBase
    {
        private const string DuplicateErrorMessage = "Please create a separate static instance for your mod.";

        /// <summary>Singleton ensures thread safety across Harmony patches. https://csharpindepth.com/Articles/Singleton</summary>
        protected static readonly HarmonyPatchBase Instance = new HarmonyPatchBase();

        private static readonly object s_locker = new object();
        private static readonly HashSet<object> s_registeredTypes = new HashSet<object>();

        /// <summary>Validate that each mod is using their own Singleton instance. https://stackoverflow.com/a/2855324/1410257</summary>
        protected HarmonyPatchBase(Type harmonyPatchType,
            string modFileName,
            bool isNotDuplicating)
        {
            if (!isNotDuplicating)
            {
                return;
            }

            SetUpPatchData(harmonyPatchType, modFileName);
            ValidateThatStaticInstanceIsNotDuplicated();
        }

        private HarmonyPatchBase()
        {
        }

        // We load our properties when applying the patch, so they will be null in the constructor
        // ReSharper disable once NullableWarningSuppressionIsUsed
        public IFileManager FileManager { get; private set; }

        // ReSharper disable once NullableWarningSuppressionIsUsed
        public ILogger Logger { get; private set; }


        /// <summary>Entry point for testing.</summary>
        public void AddLoggerTarget(ILogger logger)
        {
            Logger = logger;
        }

        protected void ApplyHarmonyPatch([NotNull] Type harmonyPatchType,
            string modFileName)
        {
            try
            {
                _ = Guard.Against.Null(harmonyPatchType, nameof(harmonyPatchType));

                HarmonyInstance harmony = HarmonyInstance.Create(modFileName);
                harmony.PatchAll(harmonyPatchType.Assembly);
            }
            catch (Exception ex)
            {
                Logger.WriteException(ex);

                throw;
            }
        }

        protected void SetUpPatchData(Type type,
            string modFileName,
            [CanBeNull] ICollection<DirectoryInfo> directories = null,
            [CanBeNull] IAngelaConversationUiTestAdapter angelaConversationUiTestAdapter = null)
        {
            if (!type.IsHarmonyPatch())
            {
                return;
            }

            try
            {
                HandleDirectories(directories, modFileName);
            }
            catch (TypeInitializationException)
            {
                // If we get a Unity exception then that means we're running this outside of Unity (i.e. unit tests), so we'll just gracefully exit
                return;
            }

            InitializeLogger(angelaConversationUiTestAdapter);
            AddDefaultLocalizedText();
            ApplyHarmonyPatch(type, modFileName);
        }

        private void HandleDirectories([CanBeNull] ICollection<DirectoryInfo> directories,
            [NotNull] string modFileName)
        {
            // Try to get Basemod directory list if we don't have one
            var directoryInfos = directories ?? Add_On.instance.DirList;
            List<IDirectoryInfo> wrappedDirectories = new List<IDirectoryInfo>();

            foreach (var directoryInfo in directoryInfos)
            {
                wrappedDirectories.Add(new DirectoryInfoAdapter(directoryInfo));
            }

            FileManager = new FileManager(modFileName, wrappedDirectories);
        }

        private void InitializeLogger(IAngelaConversationUiTestAdapter angelaConversationUiTestAdapter)
        {
            FileLoggerTarget fileLoggerTarget = new FileLoggerTarget(FileManager, "log.txt");
            Logger = new Logger(fileLoggerTarget);

#if DEBUG
            // ReSharper disable once InconsistentNaming
            const bool logToAngela = true;
#else
            var logToAngela = File.Exists("log.config");
#endif

            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            AngelaLoggerTarget angelaLoggerTarget = new AngelaLoggerTarget(logToAngela, angelaConversationUiTestAdapter);
            Logger.AddTarget(angelaLoggerTarget);
        }

        /// <summary>Needed because Basemod doesn't use a localization file as a backup, so in other languages it will default everything to "UKNOWN".</summary>
        private void AddDefaultLocalizedText()
        {
            var defaultLocalizationFile = FileManager.GetFile("Localize/en/text_en.xml");

            if (!File.Exists(defaultLocalizationFile))
            {
                return;
            }

            var xml = File.ReadAllText(defaultLocalizationFile);
            XmlDocument xmlDocument = new XmlDocument
            {
                XmlResolver = null,
            };

            XmlReaderSettings xmlSettings = new XmlReaderSettings
            {
                ProhibitDtd = true,
            };

            using (XmlReader xmlReader = XmlReader.Create(new StringReader(xml), xmlSettings))
            {
                xmlDocument.Load(xmlReader);
            }

            LocalizeTextDataLoader dataLoader = new LocalizeTextDataLoader("en");
            foreach (var keyValuePair in dataLoader.LoadText(xmlDocument))
            {
                DefaultLocalizedValues.AddOrOverwriteDefaultLocalizedValue(keyValuePair.Key, keyValuePair.Value);
            }
        }

        private void ValidateThatStaticInstanceIsNotDuplicated()
        {
            lock (s_locker)
            {
                if (s_registeredTypes.Contains(GetType()))
                {
                    throw new InvalidOperationException(DuplicateErrorMessage);
                }

                _ = s_registeredTypes.Add(GetType());
            }
        }
    }
}
