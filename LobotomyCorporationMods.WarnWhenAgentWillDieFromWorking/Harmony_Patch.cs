// SPDX-License-Identifier: MIT

#region

using System.Diagnostics.CodeAnalysis;
using LobotomyCorporationMods.Common.Implementations;

#endregion

namespace LobotomyCorporationMods.WarnWhenAgentWillDieFromWorking
{
    [SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores")]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
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
