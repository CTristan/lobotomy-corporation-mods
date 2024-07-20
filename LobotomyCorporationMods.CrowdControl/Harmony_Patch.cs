using System;
using System.Collections.Generic;
using LobotomyCorporationMods.Common.Implementations;

namespace LobotomyCorporationMods.CrowdControl
{
    // ReSharper disable once InconsistentNaming
    public sealed class Harmony_Patch : HarmonyPatchBase
    {
        public new static readonly Harmony_Patch Instance = new Harmony_Patch(true);

        public Harmony_Patch() : this(false)
        {
        }

        private Harmony_Patch(bool initialize) : base(typeof(Harmony_Patch), "LobotomyCorporationMods.CrowdControl.dll", initialize)
        {
        }

        public static Queue<Action> ActionQueue { get; } = new Queue<Action>();
    }
}
