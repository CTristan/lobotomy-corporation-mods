// SPDX-License-Identifier: MIT

using System;
using Harmony;
using LobotomyCorporationMods.BadLuckProtectionForGifts.Implementations;
using LobotomyCorporationMods.BadLuckProtectionForGifts.Interfaces;
using LobotomyCorporationMods.Common.Implementations;
using LobotomyCorporationMods.Common.Interfaces;

namespace LobotomyCorporationMods.BadLuckProtectionForGifts
{
    // ReSharper disable once InconsistentNaming
    public sealed class Harmony_Patch
    {
        private const string ModFileName = "LobotomyCorporationMods.BadLuckProtectionForGifts.dll";

        /// <summary>
        ///     Singleton ensures thread safety across the patches.
        ///     https://csharpindepth.com/Articles/Singleton
        /// </summary>
        public static readonly Harmony_Patch Instance = new Harmony_Patch(true);

        public Harmony_Patch()
        {
        }

        private Harmony_Patch(bool initialize)
        {
            if (!initialize)
            {
                return;
            }

            try
            {
                FileManager = new FileManager(ModFileName);

                try
                {
                    var harmony = HarmonyInstance.Create(ModFileName);
                    AgentWorkTracker = new AgentWorkTracker(FileManager, "BadLuckProtectionForGifts.dat");
                    harmony.PatchAll(typeof(Harmony_Patch).Assembly);
                }
                catch (Exception ex)
                {
                    FileManager.WriteToLog(ex);

                    throw;
                }
            }
            catch (TypeInitializationException)
            {
                // This exception only comes up in testing, so we ignore it
            }
        }

        public IAgentWorkTracker AgentWorkTracker { get; private set; }
        public IFileManager FileManager { get; private set; }

        /// <summary>
        ///     Entry point for testing.
        /// </summary>
        public void LoadData(IFileManager fileManager, string dataFileName)
        {
            FileManager = fileManager;
            AgentWorkTracker = new AgentWorkTracker(fileManager, dataFileName);
        }
    }
}
