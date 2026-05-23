// SPDX-License-Identifier: MIT

#region

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

#endregion

namespace LobotomyCorporationMods.Test.ModTests.DontChatMeTests.IntegrationTests
{
    /// <summary>
    ///     In-process WebSocket server bound to <c>127.0.0.1</c> on a free port. Test code drives the
    ///     server through <see cref="PushAsync" />, <see cref="CloseLatestAsync" />, and
    ///     <see cref="WaitForFrameAsync" />.
    /// </summary>
    internal sealed class LoopbackWebSocketServer : IDisposable
    {
        private readonly HttpListener _listener = new();
        private readonly CancellationTokenSource _cts = new();
        private readonly List<WebSocket> _sockets = new();
        private readonly List<string> _received = new();
        private readonly object _lock = new();
        private Task _acceptLoop;
        private bool _disposed;

        public Uri Url { get; private set; }

        public void Start()
        {
            var port = PickFreeLoopbackPort();
            var prefix = string.Format(
                CultureInfo.InvariantCulture,
                "http://127.0.0.1:{0}/mod/socket/",
                port
            );
            _listener.Prefixes.Add(prefix);
            _listener.Start();
            Url = new Uri(
                string.Format(CultureInfo.InvariantCulture, "ws://127.0.0.1:{0}/mod/socket/", port)
            );
            _acceptLoop = Task.Run(AcceptLoopAsync);
        }

        private static int PickFreeLoopbackPort()
        {
            using var probe = new TcpListener(IPAddress.Loopback, port: 0);
            probe.Start();
            try
            {
                return ((IPEndPoint)probe.LocalEndpoint).Port;
            }
            finally
            {
                probe.Stop();
            }
        }

        private async Task AcceptLoopAsync()
        {
            while (!_cts.IsCancellationRequested)
            {
                HttpListenerContext ctx;
                try
                {
                    ctx = await _listener.GetContextAsync().ConfigureAwait(false);
                }
                catch (HttpListenerException)
                {
                    break;
                }
                catch (ObjectDisposedException)
                {
                    break;
                }

                if (!ctx.Request.IsWebSocketRequest)
                {
                    ctx.Response.StatusCode = 400;
                    ctx.Response.Close();
                    continue;
                }

                HttpListenerWebSocketContext wsCtx;
                try
                {
                    wsCtx = await ctx.AcceptWebSocketAsync(subProtocol: null).ConfigureAwait(false);
                }
#pragma warning disable CA1031 // A failed upgrade is non-fatal to the accept loop; skip and keep accepting.
                catch (Exception)
#pragma warning restore CA1031
                {
                    continue;
                }

                lock (_lock)
                {
                    _sockets.Add(wsCtx.WebSocket);
                }

                _ = Task.Run(() => ReceiveLoopAsync(wsCtx.WebSocket));
            }
        }

        private async Task ReceiveLoopAsync(WebSocket socket)
        {
            var buffer = new byte[8192];
            var builder = new StringBuilder();
            try
            {
                while (socket.State == WebSocketState.Open && !_cts.IsCancellationRequested)
                {
                    WebSocketReceiveResult result;
                    try
                    {
                        result = await socket
                            .ReceiveAsync(new ArraySegment<byte>(buffer), _cts.Token)
                            .ConfigureAwait(false);
                    }
#pragma warning disable CA1031 // Receive errors (server-side abort, cancellation) end this socket's loop; nothing else to do.
                    catch (Exception)
#pragma warning restore CA1031
                    {
                        break;
                    }

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        break;
                    }

                    builder.Append(Encoding.UTF8.GetString(buffer, 0, result.Count));
                    if (!result.EndOfMessage)
                    {
                        continue;
                    }

                    var text = builder.ToString();
                    builder.Clear();
                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        lock (_lock)
                        {
                            _received.Add(text);
                        }
                    }
                }
            }
            finally
            {
                try
                {
                    socket.Dispose();
                }
#pragma warning disable CA1031 // Best-effort teardown of one client's socket; ignore disposal exceptions.
                catch (Exception)
#pragma warning restore CA1031
                {
                    // ignore
                }
            }
        }

        /// <summary>Sends a text frame on the most recently accepted socket.</summary>
        public async Task PushAsync(string json)
        {
            WebSocket target = LatestSocket();
            if (target == null || target.State != WebSocketState.Open)
            {
                throw new InvalidOperationException("No live client socket to push to.");
            }

            var bytes = Encoding.UTF8.GetBytes(json);
            await target
                .SendAsync(
                    new ArraySegment<byte>(bytes),
                    WebSocketMessageType.Text,
                    endOfMessage: true,
                    CancellationToken.None
                )
                .ConfigureAwait(false);
        }

        /// <summary>
        ///     Tears down the most recently accepted socket by sending a close frame to the client.
        ///     Models a server-initiated disconnect. The client's reconnect path runs the same way
        ///     regardless of whether the close was orderly or a network drop.
        /// </summary>
        public async Task CloseLatestAsync()
        {
            var target = LatestSocket();
            if (target == null)
            {
                throw new InvalidOperationException("No live client socket to close.");
            }

            if (target.State != WebSocketState.Open)
            {
                return;
            }

            try
            {
                await target
                    .CloseOutputAsync(
                        WebSocketCloseStatus.EndpointUnavailable,
                        "test-server-close",
                        CancellationToken.None
                    )
                    .ConfigureAwait(false);
            }
#pragma warning disable CA1031 // Best-effort close; if it fails, the test will fail with a clearer timeout downstream.
            catch (Exception)
#pragma warning restore CA1031
            {
                // ignore
            }
        }

        /// <summary>Snapshot of every text frame the server has received from clients.</summary>
        public IReadOnlyList<string> ReceivedFrames
        {
            get
            {
                lock (_lock)
                {
                    return _received.ToArray();
                }
            }
        }

        /// <summary>Number of accepted sockets across the server's lifetime (including dead ones).</summary>
        public int AcceptedSocketCount
        {
            get
            {
                lock (_lock)
                {
                    return _sockets.Count;
                }
            }
        }

        /// <summary>Polls until a received frame matches <paramref name="predicate" /> or <paramref name="timeout" /> elapses.</summary>
        public async Task<string> WaitForFrameAsync(Func<string, bool> predicate, TimeSpan timeout)
        {
            var deadline = DateTime.UtcNow + timeout;
            while (DateTime.UtcNow < deadline)
            {
                lock (_lock)
                {
                    foreach (var frame in _received)
                    {
                        if (predicate(frame))
                        {
                            return frame;
                        }
                    }
                }

                await Task.Delay(20).ConfigureAwait(false);
            }

            throw new TimeoutException(
                string.Format(
                    CultureInfo.InvariantCulture,
                    "No frame matched within {0}; received {1} frame(s).",
                    timeout,
                    ReceivedFrames.Count
                )
            );
        }

        /// <summary>Polls until <paramref name="predicate" /> returns true or <paramref name="timeout" /> elapses.</summary>
        public static async Task WaitUntilAsync(Func<bool> predicate, TimeSpan timeout)
        {
            var deadline = DateTime.UtcNow + timeout;
            while (DateTime.UtcNow < deadline)
            {
                if (predicate())
                {
                    return;
                }

                await Task.Delay(20).ConfigureAwait(false);
            }

            throw new TimeoutException("Condition was not satisfied within " + timeout);
        }

        private WebSocket LatestSocket()
        {
            lock (_lock)
            {
                for (var i = _sockets.Count - 1; i >= 0; i--)
                {
                    if (_sockets[i].State == WebSocketState.Open)
                    {
                        return _sockets[i];
                    }
                }

                return null;
            }
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            try
            {
                _cts.Cancel();
            }
            catch (ObjectDisposedException) { }

            lock (_lock)
            {
                foreach (var socket in _sockets)
                {
                    try
                    {
                        socket.Abort();
                        socket.Dispose();
                    }
#pragma warning disable CA1031 // Best-effort teardown; ignore exceptions from individual socket disposal.
                    catch (Exception)
#pragma warning restore CA1031
                    {
                        // ignore
                    }
                }

                _sockets.Clear();
            }

            try
            {
                _listener.Stop();
                ((IDisposable)_listener).Dispose();
            }
#pragma warning disable CA1031 // Best-effort teardown of the listener; ignore close-time exceptions.
            catch (Exception)
#pragma warning restore CA1031
            {
                // ignore
            }

            _cts.Dispose();
        }
    }
}
