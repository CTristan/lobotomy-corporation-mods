// SPDX-License-Identifier: MIT

using System;
using Harmony;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Attributes;
using LobotomyCorporationMods.Common.Extensions;
using LobotomyCorporationMods.DontChatMe.Extensions;
using LobotomyCorporationMods.DontChatMe.Implementations;

namespace LobotomyCorporationMods.DontChatMe.Patches
{
    [HarmonyPatch(typeof(GameManager), nameof(PrivateMethods.GameManager.Update))]
    public static class GameManagerPatchUpdate
    {
        // Track connection state to avoid spamming logs
        private static bool _wasConnected;
        private static DateTime _lastConnectionAttempt = DateTime.MinValue;
        private static readonly TimeSpan ConnectionRetryInterval = TimeSpan.FromSeconds(10);

        private static void PatchAfterUpdate([NotNull] this GameManager instance)
        {
            var webSocketClient = WebSocketClient.Instance;

            // Update the WebSocket client
            webSocketClient.Update();

            // Check if we need to attempt connection
            var now = DateTime.UtcNow;
            if (!webSocketClient.IsConnected &&
                (now - _lastConnectionAttempt) > ConnectionRetryInterval)
            {
                _lastConnectionAttempt = now;

                // Try to connect
                var connected = webSocketClient.Connect();

                // Log connection attempt only when state changes
                if (connected && !_wasConnected)
                {
                    Harmony_Patch.Instance.Logger.WriteInfo("Connected to WebSocket server: " + webSocketClient.ServerUrl);
                    _wasConnected = true;
                }
                else if (!connected && _wasConnected)
                {
                    Harmony_Patch.Instance.Logger.WriteInfo("Disconnected from WebSocket server: " + webSocketClient.ServerUrl);
                    _wasConnected = false;
                }
            }
        }

        [EntryPoint]
        // ReSharper disable once InconsistentNaming
        public static void Postfix([NotNull] GameManager __instance)
        {
            try
            {
                Guard.Against.Null(__instance, nameof(__instance));
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
