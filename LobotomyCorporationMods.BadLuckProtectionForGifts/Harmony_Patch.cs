// SPDX-License-Identifier: MIT

using System.Reflection;
using Harmony;
using LobotomyCorporationMods.BadLuckProtectionForGifts.Implementations;
using LobotomyCorporationMods.BadLuckProtectionForGifts.Interfaces;
using LobotomyCorporationMods.Common.Implementations;
using LobotomyCorporationMods.Common.Interfaces;

namespace LobotomyCorporationMods.BadLuckProtectionForGifts
{
#pragma warning disable CA1707
    // ReSharper disable once InconsistentNaming
    public sealed class Harmony_Patch
    {
        private const string ModFileName = "LobotomyCorporationMods.BadLuckProtectionForGifts.dll";

        /// <summary>
        ///     https://csharpindepth.com/Articles/Singleton
        /// </summary>
        static Harmony_Patch()
        {
        }

        private Harmony_Patch()
        {
            try
            {
                FileManager = new FileManager(ModFileName);
                var harmony = HarmonyInstance.Create(ModFileName);
                AgentWorkTracker = new AgentWorkTracker(FileManager, "BadLuckProtectionForGifts.dat");
                harmony.PatchAll(Assembly.GetExecutingAssembly());
            }
            catch
            {
                // ignored
            }
        }

        public static Harmony_Patch Instance { get; } = new Harmony_Patch();
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
#pragma warning restore CA1707
