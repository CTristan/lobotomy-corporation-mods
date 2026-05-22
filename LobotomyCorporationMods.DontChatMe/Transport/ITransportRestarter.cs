// SPDX-License-Identifier: MIT

namespace LobotomyCorporationMods.DontChatMe.Transport
{
    /// <summary>
    ///     One-method seam over <see cref="WebSocketTransport.Restart" /> so the Settings UI
    ///     can request a reconnect without taking a hard reference on the concrete transport.
    /// </summary>
    public interface ITransportRestarter
    {
        /// <summary>
        ///     Stops the current connection and reconnects with the current config. If
        ///     <c>Enabled</c> is false on the config, the transport stops only.
        /// </summary>
        void Restart();
    }
}
