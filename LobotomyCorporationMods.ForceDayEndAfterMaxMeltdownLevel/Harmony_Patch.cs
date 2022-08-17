// SPDX-License-Identifier: MIT

using System;
using Harmony;
using LobotomyCorporationMods.Common.Implementations;
using LobotomyCorporationMods.Common.Interfaces;

#pragma warning disable CA1707
namespace LobotomyCorporationMods.ForceDayEndAfterMaxMeltdownLevel
{
    public sealed class Harmony_Patch
    {
        private const string ModFileName = "LobotomyCorporationMods.ForceDayEndAfterMaxMeltdownLevel.dll";

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

        internal IFileManager FileManager { get; private set; }

        /// <summary>
        ///     Entry point for testing.
        /// </summary>
        public void LoadData(IFileManager fileManager)
        {
            FileManager = fileManager;
        }
    }
}
#pragma warning restore CA1707
