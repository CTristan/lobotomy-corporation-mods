// SPDX-License-Identifier: MIT

#region

using LobotomyCorporation.Mods.Common;

#endregion

namespace LobotomyCorporationMods.NotifyWhenAgentReceivesGift
{
    // ReSharper disable once InconsistentNaming
    public sealed class Harmony_Patch : HarmonyPatchBase<Harmony_Patch>
    {
        public static readonly Harmony_Patch Instance = new Harmony_Patch(true);

        public Harmony_Patch() { }

        private Harmony_Patch(bool initialize)
            : base(initialize) { }
    }
}
