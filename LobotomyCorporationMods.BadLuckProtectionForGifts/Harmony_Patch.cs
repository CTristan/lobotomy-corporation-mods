// SPDX-License-Identifier: MIT

#region

using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.InteropServices;
using LobotomyCorporationMods.BadLuckProtectionForGifts.Implementations;
using LobotomyCorporationMods.BadLuckProtectionForGifts.Interfaces;
using LobotomyCorporationMods.Common.Implementations;

#endregion

[assembly: AssemblyVersion("1.0.*")]
[assembly: CLSCompliant(false)]
[assembly: ComVisible(false)]

namespace LobotomyCorporationMods.BadLuckProtectionForGifts
{
    // ReSharper disable once InconsistentNaming
    [SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores")]
    public sealed class Harmony_Patch : HarmonyPatchBase
    {
        private const string ModFileName = "LobotomyCorporationMods.BadLuckProtectionForGifts.dll";

        public static new readonly Harmony_Patch Instance = new(true);

        public Harmony_Patch()
            : this(false)
        {
        }

        private Harmony_Patch(bool initialize)
            : base(initialize)
        {
            if (initialize)
            {
                InitializePatchData(typeof(Harmony_Patch), ModFileName);
                AgentWorkTracker = new AgentWorkTracker(FileManager, "BadLuckProtectionForGifts.dat");
            }
        }

        internal IAgentWorkTracker AgentWorkTracker { get; private set; }

        /// <summary>
        ///     Entry point for testing.
        /// </summary>
        public void LoadTracker(IAgentWorkTracker agentWorkTracker)
        {
            AgentWorkTracker = agentWorkTracker;
        }
    }
}
