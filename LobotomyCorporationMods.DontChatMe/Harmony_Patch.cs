// SPDX-License-Identifier: MIT

#region

using System;
using LobotomyCorporationMods.Common.Implementations;
using LobotomyCorporationMods.DontChatMe.Configuration;
using LobotomyCorporationMods.DontChatMe.Implementations;
using LobotomyCorporationMods.DontChatMe.Interfaces;

#endregion

namespace LobotomyCorporationMods.DontChatMe
{
    // ReSharper disable once InconsistentNaming
    public sealed class Harmony_Patch : HarmonyPatchBase
    {
        public new static readonly Harmony_Patch Instance = new Harmony_Patch(true);

        private readonly object _syncLock = new object();

        private IWebSocketClient _webSocketClient;

        public Harmony_Patch() : this(false)
        {
        }

        private Harmony_Patch(bool initialize) : base(typeof(Harmony_Patch), "LobotomyCorporationMods.DontChatMe.dll",
            initialize)
        {
        }

        /// <summary>
        ///     Gets the WebSocket client instance.
        /// </summary>
        public IWebSocketClient WebSocketClient
        {
            get
            {
                if (_webSocketClient != null)
                {
                    return _webSocketClient;
                }

                lock (_syncLock)
                {
                    try
                    {
                        // Initialize the WebSocket client with the server URL
                        var serverPath = WebSocketSettings.ServerPath;

                        _webSocketClient = new WebSocketClient(serverPath);

                        // Log initialization
                        Logger.Log(
                            "DontChatMe WebSocket client initialized with server URL: " +
                            serverPath);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogException(ex);
                        throw;
                    }
                }

                return _webSocketClient;
            }
        }
    }
}
