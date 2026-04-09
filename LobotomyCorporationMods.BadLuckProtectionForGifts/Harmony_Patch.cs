// SPDX-License-Identifier: MIT

#region

using System;
using System.Security;
using LobotomyCorporation.Mods.Common.Implementations;
using LobotomyCorporationMods.BadLuckProtectionForGifts.Implementations;
using LobotomyCorporationMods.BadLuckProtectionForGifts.Interfaces;

#endregion

namespace LobotomyCorporationMods.BadLuckProtectionForGifts
{
    // ReSharper disable once InconsistentNaming
    public sealed class Harmony_Patch : HarmonyPatchBase
    {
        public static new readonly Harmony_Patch Instance = new Harmony_Patch(true);

        public Harmony_Patch()
            : this(false) { }

        private Harmony_Patch(bool initialize)
            : base(
                typeof(Harmony_Patch),
                "LobotomyCorporationMods.BadLuckProtectionForGifts.dll",
                initialize
            )
        {
            AgentWorkTracker = new AgentWorkTracker(FileManager, "BadLuckProtectionForGifts.dat");
            Config = InitializeConfig();
        }

        // ReSharper disable once NullableWarningSuppressionIsUsed
        // We load the tracker later on when needed, so this should never be actually null
        internal IAgentWorkTracker AgentWorkTracker { get; }

        /// <summary>
        ///     Set by Prefix on FinishWorkSuccessfully, read by GetProb Postfix, cleared by
        ///     FinishWorkSuccessfully Postfix. Unity is single-threaded so no race conditions.
        /// </summary>
        internal long? CurrentWorkingAgentId { get; set; }

        internal IBadLuckProtectionConfig Config { get; }

        private static IBadLuckProtectionConfig InitializeConfig()
        {
            try
            {
                return new BadLuckProtectionConfig();
            }
            catch (TypeLoadException)
            {
                // ConfigurationManager DLL is not installed
                return new DefaultBadLuckProtectionConfig();
            }
            catch (SecurityException)
            {
                // Unity runtime is unavailable (e.g. in tests)
                return new DefaultBadLuckProtectionConfig();
            }
        }
    }
}
