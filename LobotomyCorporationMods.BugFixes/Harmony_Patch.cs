// SPDX-License-Identifier: MIT

#region

using LobotomyCorporationMods.Common.Implementations;

#endregion

namespace LobotomyCorporationMods.BugFixes
{
    // ReSharper disable once InconsistentNaming
    public sealed class Harmony_Patch : HarmonyPatchBase
    {
        public new static readonly Harmony_Patch Instance = new Harmony_Patch(true);

        public Harmony_Patch() : this(false)
        {
        }

        private Harmony_Patch(bool initialize) : base(typeof(Harmony_Patch), "LobotomyCorporationMods.BugFixes.dll", initialize)
        {
        }
    }
}
