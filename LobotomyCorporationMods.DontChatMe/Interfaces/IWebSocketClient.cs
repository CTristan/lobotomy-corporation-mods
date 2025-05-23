// SPDX-License-Identifier: MIT

using System;
using LobotomyCorporationMods.DontChatMe.Models.EffectMessages;
using WebSocketSharp;

namespace LobotomyCorporationMods.DontChatMe.Interfaces
{
    /// <summary>
    ///     Interface for WebSocket client operations.
    /// </summary>
    public interface IWebSocketClient
    {
        bool IsAlive { get; }
        event EventHandler<MessageEventArgs> MessageReceived;
        event EventHandler<CloseEventArgs> ConnectionClosed;
        event EventHandler<ErrorEventArgs> ErrorOccurred;
        void Connect();
        void Close();
        void RegisterEffectHandler(string effectId, Func<EffectRequest, EffectResponse> handler);
    }
}
