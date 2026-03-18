// SPDX-License-Identifier: MIT

#region

using Hemocode.BadLuckProtectionForGifts.Implementations;
using Hemocode.BadLuckProtectionForGifts.Interfaces;
using Hemocode.Common.Implementations;

#endregion

namespace Hemocode.BadLuckProtectionForGifts
{
    // ReSharper disable once InconsistentNaming
    public sealed class Harmony_Patch : HarmonyPatchBase
    {
        public static new readonly Harmony_Patch Instance = new Harmony_Patch(true);

        public Harmony_Patch() : this(false)
        {
        }

        private Harmony_Patch(bool initialize) : base(typeof(Harmony_Patch), "Hemocode.BadLuckProtectionForGifts.dll", initialize)
        {
            AgentWorkTracker = new AgentWorkTracker(FileManager, "BadLuckProtectionForGifts.dat");
        }

        // ReSharper disable once NullableWarningSuppressionIsUsed
        // We load the tracker later on when needed, so this should never be actually null
        internal IAgentWorkTracker AgentWorkTracker { get; }
    }
}
