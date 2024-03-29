﻿// SPDX-License-Identifier: MIT

#region

using System;
using System.Reflection;
using System.Runtime.InteropServices;
using LobotomyCorporationMods.Common.Implementations;

#endregion

[assembly: AssemblyVersion("1.0.*")]
[assembly: CLSCompliant(false)]
[assembly: ComVisible(false)]

namespace LobotomyCorporationMods.WarnWhenAgentWillDieFromWorking
{
    public sealed class Harmony_Patch : HarmonyPatchBase
    {
        private const string ModFileName = "LobotomyCorporationMods.WarnWhenAgentWillDieFromWorking.dll";

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
            }
        }
    }
}
