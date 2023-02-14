// SPDX-License-Identifier: MIT

#region

using System;
using System.Collections.Generic;
using System.IO;
using Harmony;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Extensions;
using LobotomyCorporationMods.Common.Interfaces;

#endregion

namespace LobotomyCorporationMods.Common.Implementations
{
    public class HarmonyPatchBase
    {
        private const string DuplicateErrorMessage = "Please create a separate static instance for your mod.";
        private static readonly object s_locker = new();
        private static readonly HashSet<object> s_registeredTypes = new();

        /// <summary>
        ///     Singleton ensures thread safety across Harmony patches.
        ///     https://csharpindepth.com/Articles/Singleton
        /// </summary>
        protected static readonly HarmonyPatchBase Instance = new();

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

        protected IFileManager FileManager { get; private set; }

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

        protected void InitializePatchData([NotNull] Type harmonyPatchType, [NotNull] string modFileName, List<DirectoryInfo> directoryList = null)
        {
            Guard.Against.Null(harmonyPatchType, nameof(harmonyPatchType));

            if (harmonyPatchType.IsHarmonyPatch())
            {
                try
                {
                    // Try to get Basemod directory list if we don't have one
                    directoryList ??= Add_On.instance.DirList;
                }
                catch (SystemException)
                {
                    // If we get a SystemException then that means we're running this outside of Unity (i.e. unit tests), so we'll just gracefully exit
                    return;
                }

                FileManager = new FileManager(modFileName, directoryList);
                Logger = new Logger(FileManager);

                ApplyHarmonyPatch(harmonyPatchType, modFileName);
            }
        }

        protected void ApplyHarmonyPatch([NotNull] Type harmonyPatchType, string modFileName)
        {
            try
            {
                Guard.Against.Null(harmonyPatchType, nameof(harmonyPatchType));

                var harmony = HarmonyInstance.Create(modFileName);
                harmony.PatchAll(harmonyPatchType.Assembly);
            }
            catch (Exception ex)
            {
                Logger.WriteToLog(ex);

                throw;
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
