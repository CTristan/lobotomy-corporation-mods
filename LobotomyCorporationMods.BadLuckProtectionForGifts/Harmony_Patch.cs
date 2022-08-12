// SPDX-License-Identifier: MIT

using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Harmony;
using JetBrains.Annotations;
using LobotomyCorporationMods.BadLuckProtectionForGifts.Implementations;
using LobotomyCorporationMods.BadLuckProtectionForGifts.Interfaces;
using LobotomyCorporationMods.Common.Implementations;
using LobotomyCorporationMods.Common.Interfaces;

namespace LobotomyCorporationMods.BadLuckProtectionForGifts
{
    [SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores")]
    // ReSharper disable once InconsistentNaming
    public sealed class Harmony_Patch
    {
        private const string ModFileName = "LobotomyCorporationMods.BadLuckProtectionForGifts.dll";

        private static IAgentWorkTracker s_agentWorkTracker;
        private static IFileManager s_fileManager;

        /// <summary>
        ///     Do not use for testing as it causes an exception. Use the other constructor instead.
        /// </summary>
        public Harmony_Patch()
        {
            s_fileManager = new FileManager(ModFileName);

            try
            {
                var harmony = HarmonyInstance.Create(ModFileName);
                s_agentWorkTracker = new AgentWorkTracker(s_fileManager, "BadLuckProtectionForGifts.dat");
                harmony.PatchAll(Assembly.GetExecutingAssembly());
            }
            catch (Exception ex)
            {
                s_fileManager.WriteToLog(ex);
            }
        }

        /// <summary>
        ///     Entry point for testing.
        /// </summary>
        public Harmony_Patch(IFileManager fileManager, string dataFileName)
        {
            s_fileManager = fileManager;
            s_agentWorkTracker = new AgentWorkTracker(fileManager, dataFileName);
        }

        [NotNull]
        public static IAgentWorkTracker GetAgentWorkTracker()
        {
            return s_agentWorkTracker;
        }

        [NotNull]
        public static IFileManager GetFileManager()
        {
            return s_fileManager;
        }
    }
}
