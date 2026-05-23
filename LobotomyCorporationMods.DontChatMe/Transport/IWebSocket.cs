// SPDX-License-Identifier: MIT

#region

using System;
using System.Diagnostics.CodeAnalysis;

#endregion

namespace LobotomyCorporationMods.DontChatMe.Transport
{
    /// <summary>
    ///     Narrow contract over the underlying WebSocket library so the protocol layer is testable
    ///     against a fake. The production implementation lives in <c>WebSocketSharpAdapter</c>.
    /// </summary>
    [SuppressMessage(
        "Design",
        "CA1003",
        Justification = "Events are an assembly-private callback channel; the Action<T> form keeps subscribers terse without adding EventArgs ceremony."
    )]
    public interface IWebSocket : IDisposable
    {
        /// <summary>Raised on a worker thread when the socket transitions to OPEN.</summary>
        event Action Opened;

        /// <summary>
        ///     Raised on a worker thread for each inbound text frame. Binary frames are dropped before
        ///     this event fires.
        /// </summary>
        event Action<string> MessageReceived;

        /// <summary>
        ///     Raised on a worker thread when the socket closes for any reason. The first
        ///     argument is the close code (4001 means a newer connection has superseded this
        ///     one; the transport stops reconnecting on that signal). The second argument is
        ///     the human-readable close reason or <see cref="string.Empty" /> when none.
        /// </summary>
        event Action<ushort, string> Closed;

        /// <summary>Raised on a worker thread when the underlying library reports a transport error.</summary>
        event Action<Exception> ErrorOccurred;

        /// <summary>Synchronously opens the socket; returns when the handshake completes (or fails).</summary>
        void Connect();

        /// <summary>Sends a text frame. Thread-safe.</summary>
        void Send(string text);

        /// <summary>Closes the socket; safe to call from any state.</summary>
        void Close();

        /// <summary>True once <see cref="Opened" /> has fired and before <see cref="Closed" /> does.</summary>
        bool IsAlive { get; }
    }
}
