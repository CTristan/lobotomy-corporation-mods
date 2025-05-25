// SPDX-License-Identifier: MIT

using System;
using LobotomyCorporationMods.DontChatMe.EventArgs;
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
        void Close();
        void Connect();
        event EventHandler<CloseEventArgs> ConnectionClosed;
        event EventHandler<ConnectionStatusChangedEventArgs> ConnectionStatusChanged;
        event EventHandler<ErrorEventArgs> ErrorOccurred;
        event EventHandler<MessageEventArgs> MessageReceived;
        void RegisterEffectHandler(string effectId, Func<EffectRequest, EffectResponse> handler);
    }
}
