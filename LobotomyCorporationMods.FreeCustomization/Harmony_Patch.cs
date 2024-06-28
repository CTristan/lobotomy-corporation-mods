// SPDX-License-Identifier: MIT

#region

using System;
using System.Runtime.InteropServices;
using LobotomyCorporationMods.Common.Implementations;

#endregion

[assembly: CLSCompliant(false)]
[assembly: ComVisible(false)]

namespace LobotomyCorporationMods.FreeCustomization
{
    // ReSharper disable once InconsistentNaming
    public sealed class Harmony_Patch : HarmonyPatchBase
    {
        public new static readonly Harmony_Patch Instance = new Harmony_Patch(true);

        public Harmony_Patch()
            : this(false)
        {
        }

        private Harmony_Patch(bool initialize)
            : base(typeof(Harmony_Patch), "LobotomyCorporationMods.FreeCustomization.dll", initialize)
        {
        }
    }
}
