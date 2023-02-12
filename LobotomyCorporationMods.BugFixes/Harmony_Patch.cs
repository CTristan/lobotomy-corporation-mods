// SPDX-License-Identifier: MIT

using System.Diagnostics.CodeAnalysis;
using LobotomyCorporationMods.Common.Implementations;

namespace LobotomyCorporationMods.BugFixes
{
    [SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores")]
    // ReSharper disable once InconsistentNaming
    public sealed class Harmony_Patch : HarmonyPatchBase
    {
        private const string ModFileName = "LobotomyCorporationMods.BugFixes.dll";

        public new static readonly Harmony_Patch Instance = new Harmony_Patch(true);

        public Harmony_Patch() : this(false)
        {
        }

        private Harmony_Patch(bool initialize) : base(initialize)
        {
            if (initialize)
            {
                InitializePatchData(typeof(Harmony_Patch), ModFileName);
            }
        }
    }
}
