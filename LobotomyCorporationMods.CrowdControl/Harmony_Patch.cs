using System;
using System.Collections.Generic;
using System.Threading;
using LobotomyCorporationMods.Common.Implementations;
using LobotomyCorporationMods.CrowdControl.CrowdControl;

namespace LobotomyCorporationMods.CrowdControl
{
    // ReSharper disable once InconsistentNaming
    public sealed class Harmony_Patch : HarmonyPatchBase
    {
        public new static readonly Harmony_Patch Instance = new Harmony_Patch(true);

        // ReSharper disable once UnusedMember.Global
        public Harmony_Patch() : this(false)
        {
        }

        private Harmony_Patch(bool initialize) : base(typeof(Harmony_Patch), "LobotomyCorporationMods.CrowdControl.dll", initialize)
        {
            try
            {
                Logger.LogInfo("Starting CrowdControl mod...");

                var client = new CrowdControlClient();
                new Thread(client.NetworkLoop).Start();
                new Thread(client.RequestLoop).Start();
            }
            catch (Exception e)
            {
                Logger.LogInfo($"CC Init Error: {e}");
                throw;
            }
        }

        public static Queue<Action> ActionQueue { get; } = new Queue<Action>();
    }
}
