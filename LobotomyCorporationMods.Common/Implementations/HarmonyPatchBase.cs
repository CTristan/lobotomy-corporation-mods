// SPDX-License-Identifier: MIT

#region

using System;
using System.Collections.Generic;
using System.IO;
using Harmony;
using LobotomyCorporationMods.Common.Extensions;
using LobotomyCorporationMods.Common.Interfaces;

#endregion

namespace LobotomyCorporationMods.Common.Implementations
{
    public class HarmonyPatchBase
    {
        private const string DuplicateErrorMessage = "Please create a separate static instance for your mod.";

        /// <summary>
        ///     Singleton ensures thread safety across Harmony patches.
        ///     https://csharpindepth.com/Articles/Singleton
        /// </summary>
        protected static readonly HarmonyPatchBase Instance = new HarmonyPatchBase();

        private static readonly object s_locker = new object();
        private static readonly HashSet<object> s_registeredTypes = new HashSet<object>();

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

        // We load our properties when applying the patch, so they will be null in the constructor
        // ReSharper disable once NullableWarningSuppressionIsUsed
        protected IFileManager FileManager { get; private set; }

        // ReSharper disable once NullableWarningSuppressionIsUsed
        public ILogger Logger { get; private set; }


        protected void ApplyHarmonyPatch(Type harmonyPatchType, string modFileName)
        {
            try
            {
                if (harmonyPatchType is null)
                {
                    throw new ArgumentNullException(nameof(harmonyPatchType));
                }

                var harmony = HarmonyInstance.Create(modFileName);
                harmony.PatchAll(harmonyPatchType.Assembly);
            }
            catch (Exception ex)
            {
                Logger.WriteToLog(ex);

                throw;
            }
        }

        protected void InitializePatchData(Type harmonyPatchType, string modFileName)
        {
            InitializePatchData(harmonyPatchType, modFileName, null);
        }

        protected void InitializePatchData(Type harmonyPatchType, string modFileName,
            ICollection<DirectoryInfo> directoryList)
        {
            if (harmonyPatchType.IsHarmonyPatch())
            {
                try
                {
                    // Try to get Basemod directory list if we don't have one
                    directoryList = directoryList ?? Add_On.instance.DirList;
                }
                catch (Exception exception) when (exception is SystemException)
                {
                    // If we get a Unity exception then that means we're running this outside of Unity (i.e. unit tests), so we'll just gracefully exit
                    return;
                }

                FileManager = new FileManager(modFileName, directoryList);
                Logger = new Logger(FileManager);

                ApplyHarmonyPatch(harmonyPatchType, modFileName);
            }
        }

        /// <summary>
        ///     Entry point for testing.
        /// </summary>
        public void LoadData(ILogger logger)
        {
            Logger = logger;
        }

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
    }
}
