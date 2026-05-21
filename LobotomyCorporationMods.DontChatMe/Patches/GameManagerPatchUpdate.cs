// SPDX-License-Identifier: MIT

#region

using System;
using System.Diagnostics.CodeAnalysis;
using Harmony;
using JetBrains.Annotations;
using LobotomyCorporation.Mods.Common;

#endregion

namespace LobotomyCorporationMods.DontChatMe.Patches
{
    /// <summary>
    ///     Per-frame hook into the game's main loop.
    ///     Drains the inbound effect queue onto the Unity main thread and lazily opens the WebSocket
    ///     on the first tick.
    /// </summary>
    [HarmonyPatch(typeof(GameManager), "Update")]
    public static class GameManagerPatchUpdate
    {
        internal static void PatchAfterUpdate(this GameManager instance)
        {
            ThrowHelper.ThrowIfNull(instance, nameof(instance));
            Harmony_Patch.Instance.EnsureTransportStarted();
            Harmony_Patch.Instance.Pump.Tick();
        }

        [EntryPoint]
        [ExcludeFromCodeCoverage(Justification = Messages.UnityCodeCoverageJustification)]
        // ReSharper disable once InconsistentNaming
        public static void Postfix([NotNull] GameManager __instance)
        {
            try
            {
                __instance.PatchAfterUpdate();
            }
            catch (Exception ex)
            {
                Harmony_Patch.Instance.Logger.WriteException(ex);
                throw;
            }
        }
    }
}
