// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.IO;
using Harmony;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Extensions;
using LobotomyCorporationMods.Common.Interfaces;

namespace LobotomyCorporationMods.Common.Implementations
{
    public class HarmonyPatchBase
    {
        private const string DuplicateErrorMessage = "Please create a separate static instance for your mod.";
        private static readonly object s_locker = new object();
        private static readonly HashSet<object> s_registeredTypes = new HashSet<object>();

        /// <summary>
        ///     Singleton ensures thread safety across Harmony patches.
        ///     https://csharpindepth.com/Articles/Singleton
        /// </summary>
        public static readonly HarmonyPatchBase Instance = new HarmonyPatchBase();

        /// <summary>
        ///     Validate that each mod is using their own Singleton instance.
        ///     https://stackoverflow.com/a/2855324/1410257
        /// </summary>
        protected HarmonyPatchBase(bool isNotDuplicating)
        {
            if (isNotDuplicating)
            {
                ValidateThatStaticInstanceIsNotDuplicated();
            }
        }

        private HarmonyPatchBase()
        {
        }

        protected IFileManager FileManager { get; set; }
        public ILogger Logger { get; private set; }

        private void ValidateThatStaticInstanceIsNotDuplicated()
        {
            lock (s_locker)
            {
                if (s_registeredTypes.Contains(GetType()))
                {
                    throw new InvalidOperationException(DuplicateErrorMessage);
                }

                s_registeredTypes.Add(GetType());
            }
        }

        protected void InitializePatchData([NotNull] Type harmonyPatchType, [NotNull] string modFileName)
        {
            Guard.Against.Null(harmonyPatchType, nameof(harmonyPatchType));

            if (harmonyPatchType.IsHarmonyPatch())
            {
                List<DirectoryInfo> directoryList;

                try
                {
                    // Try to get Basemod's directory list
                    directoryList = Add_On.instance.DirList;
                }
                catch (SystemException)
                {
                    // If we get a SystemException then that means we're running this outside of Unity (i.e. unit tests), so we'll just gracefully exit
                    return;
                }

                FileManager = new FileManager(modFileName, directoryList);
                Logger = new Logger(FileManager);

                try
                {
                    var harmony = HarmonyInstance.Create(modFileName);
                    harmony.PatchAll(harmonyPatchType.Assembly);
                }
                catch (Exception ex)
                {
                    Logger.WriteToLog(ex);

                    throw;
                }
            }
        }

        /// <summary>
        ///     Entry point for testing.
        /// </summary>
        public void LoadData(ILogger logger)
        {
            Logger = logger;
        }
    }
}
