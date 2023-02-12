// SPDX-License-Identifier: MIT

using System.Diagnostics.CodeAnalysis;
using LobotomyCorporationMods.Common.Implementations;

namespace LobotomyCorporationMods.WarnWhenAgentWillDieFromWorking
{
    [SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores")]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public sealed class Harmony_Patch : HarmonyPatchBase
    {
        private const string ModFileName = "LobotomyCorporationMods.WarnWhenAgentWillDieFromWorking.dll";

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
