// SPDX-License-Identifier: MIT
namespace LobotomyCorporationMods.Common.EventArgs
{
    using System;

    /// <summary>
    ///     Event arguments for when the WebSocket connection status changes.
    /// </summary>
    public class ConnectionStatusChangedEventArgs : EventArgs
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ConnectionStatusChangedEventArgs" /> class.
        /// </summary>
        /// <param name="isConnected"><c>true</c> if the WebSocket client is connected.</param>
        public ConnectionStatusChangedEventArgs(bool isConnected)
        {
            this.IsConnected = isConnected;
        }

        /// <summary>
        ///     Gets a value indicating whether the WebSocket client is currently connected.
        /// </summary>
        /// <value>
        ///     <c>true</c> if the WebSocket client is connected; otherwise, <c>false</c>.
        /// </value>
        public bool IsConnected { get; }
    }
}
