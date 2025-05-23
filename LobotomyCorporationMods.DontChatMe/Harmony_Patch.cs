// SPDX-License-Identifier: MIT

#region

using System;
using LobotomyCorporationMods.Common.Implementations;
using LobotomyCorporationMods.DontChatMe.Implementations;
using LobotomyCorporationMods.DontChatMe.Interfaces;

#endregion

namespace LobotomyCorporationMods.DontChatMe
{
    // ReSharper disable once InconsistentNaming
    public sealed class Harmony_Patch : HarmonyPatchBase
    {
        // Default WebSocket server URL - can be changed in configuration
        private const string DefaultWebSocketServerUrl = "ws://localhost:8002/effect/ws";

        public new static readonly Harmony_Patch Instance = new Harmony_Patch(true);

        public Harmony_Patch() : this(false)
        {
        }

        private Harmony_Patch(bool initialize) : base(typeof(Harmony_Patch), "LobotomyCorporationMods.DontChatMe.dll",
            initialize)
        {
            try
            {
                // Initialize the WebSocket client with the default server URL
                WebSocketClient = new WebSocketClient(DefaultWebSocketServerUrl);

                // Log initialization
                Logger.Log(
                    "DontChatMe WebSocket client initialized with server URL: " + DefaultWebSocketServerUrl);

                WebSocketClient.Connect();

                Logger.Log("Connected to WebSocket server.");
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
                throw;
            }
        }

        /// <summary>
        ///     Gets the WebSocket client instance.
        /// </summary>
        public IWebSocketClient WebSocketClient { get; }
    }
}
