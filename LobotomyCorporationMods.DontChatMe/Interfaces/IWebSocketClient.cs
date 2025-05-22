// SPDX-License-Identifier: MIT

namespace LobotomyCorporationMods.DontChatMe.Interfaces
{
    /// <summary>
    ///     Interface for WebSocket client operations.
    /// </summary>
    public interface IWebSocketClient
    {
        void Connect();
        void Close();
    }
}
