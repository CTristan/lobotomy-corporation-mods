// SPDX-License-Identifier: MIT

#region

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
        private const string DefaultWebSocketServerUrl = "ws://localhost:8000/ws";

        public new static readonly Harmony_Patch Instance = new Harmony_Patch(true);

        public Harmony_Patch() : this(false)
        {
        }

        private Harmony_Patch(bool initialize) : base(typeof(Harmony_Patch), "LobotomyCorporationMods.DontChatMe.dll",
            initialize)
        {
            // Initialize the WebSocket client with the default server URL
            WebSocketClient = new WebSocketClient(DefaultWebSocketServerUrl);

            // Log initialization
            Logger.WriteInfo("DontChatMe WebSocket client initialized with server URL: " + DefaultWebSocketServerUrl);
        }

        /// <summary>
        ///     Gets the WebSocket client instance.
        /// </summary>
        public IWebSocketClient WebSocketClient { get; }
    }
}
