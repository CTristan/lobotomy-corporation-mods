// SPDX-License-Identifier: MIT

using System;
using LobotomyCorporationMods.DontChatMe.Models.EffectMessages;

namespace LobotomyCorporationMods.DontChatMe.Interfaces
{
    /// <summary>
    ///     Interface for WebSocket client operations.
    /// </summary>
    public interface IWebSocketClient
    {
        void Connect();
        void Close();
        void RegisterEffectHandler(string effectId, Func<EffectRequest, EffectResponse> handler);
    }
}
