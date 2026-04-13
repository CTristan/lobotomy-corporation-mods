// SPDX-License-Identifier: MIT

#region

using LobotomyCorporation.Mods.Common;
using LobotomyCorporationMods.BadLuckProtectionForGifts.Implementations;
using LobotomyCorporationMods.BadLuckProtectionForGifts.Interfaces;

#endregion

namespace LobotomyCorporationMods.BadLuckProtectionForGifts
{
    // ReSharper disable once InconsistentNaming
    public sealed class Harmony_Patch : HarmonyPatchBase<Harmony_Patch>
    {
        public static readonly Harmony_Patch Instance = new Harmony_Patch(true);

        public Harmony_Patch() { }

        private Harmony_Patch(bool initialize)
            : base(initialize)
        {
            AgentWorkTracker = new AgentWorkTracker(FileManager, "BadLuckProtectionForGifts.dat");
            Config = new BadLuckProtectionConfig();
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
    }
}
