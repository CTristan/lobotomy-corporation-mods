﻿// SPDX-License-Identifier: MIT

#region

using System;
using System.Runtime.InteropServices;
using LobotomyCorporationMods.BadLuckProtectionForGifts.Implementations;
using LobotomyCorporationMods.BadLuckProtectionForGifts.Interfaces;
using LobotomyCorporationMods.Common.Implementations;

#endregion

[assembly: CLSCompliant(false)]
[assembly: ComVisible(false)]

namespace LobotomyCorporationMods.BadLuckProtectionForGifts
{
    // ReSharper disable once InconsistentNaming
    public sealed class Harmony_Patch : HarmonyPatchBase
    {
        private const string ModFileName = "LobotomyCorporationMods.BadLuckProtectionForGifts.dll";

        public new static readonly Harmony_Patch Instance = new Harmony_Patch(true);

        public Harmony_Patch()
            : this(false)
        {
        }

        private Harmony_Patch(bool initialize)
            : base(initialize)
        {
            if (!initialize)
            {
                return;
            }

            InitializePatchData(typeof(Harmony_Patch), ModFileName);
            AgentWorkTracker = new AgentWorkTracker(FileManager, "BadLuckProtectionForGifts.dat");
        }

        // ReSharper disable once NullableWarningSuppressionIsUsed
        // We load the tracker later on when needed, so this should never be actually null
        internal IAgentWorkTracker AgentWorkTracker { get; }
    }
}
