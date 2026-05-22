// SPDX-License-Identifier: MIT

#region

using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LobotomyCorporationMods.DontChatMe.Transport;

#endregion

namespace LobotomyCorporationMods.Test.ModTests.DontChatMeTests.IntegrationTests
{
    /// <summary>
    ///     Test-only <see cref="IWebSocket" /> implementation backed by
    ///     <see cref="ClientWebSocket" />. Lets the loopback integration tests exercise the full
    ///     <see cref="WebSocketTransport" /> state machine against a real WS server, with real
    ///     async event delivery on a worker thread (matching the production contract).
    /// </summary>
    internal sealed class ClientWebSocketAdapter : IWebSocket
    {
        private readonly Uri _url;
        private readonly ClientWebSocket _client = new();
        private readonly CancellationTokenSource _cts = new();
        private readonly SemaphoreSlim _sendSemaphore = new(initialCount: 1, maxCount: 1);
        private volatile bool _isAlive;
        private bool _disposed;

        public ClientWebSocketAdapter(Uri url)
        {
            _url = url ?? throw new ArgumentNullException(nameof(url));
        }

        public event Action Opened;
        public event Action<string> MessageReceived;
        public event Action Closed;
        public event Action<Exception> ErrorOccurred;

        public bool IsAlive => _isAlive;

        public void Connect()
        {
            try
            {
                _client.ConnectAsync(_url, _cts.Token).GetAwaiter().GetResult();
            }
#pragma warning disable CA1031 // Any connect-time exception must surface as a normal disconnect so the transport's reconnect path kicks in.
            catch (Exception ex)
#pragma warning restore CA1031
            {
                ErrorOccurred?.Invoke(ex);
                _isAlive = false;
                Closed?.Invoke();
                return;
            }

            _isAlive = true;
            // Fire Opened from the worker thread to match the production contract:
            // "Raised on a worker thread when the socket transitions to OPEN."
            _ = Task.Run(async () =>
            {
                Opened?.Invoke();
                await ReceiveLoopAsync().ConfigureAwait(false);
            });
        }

        private async Task ReceiveLoopAsync()
        {
            var buffer = new byte[8192];
            var builder = new StringBuilder();
            try
            {
                while (!_cts.IsCancellationRequested && _client.State == WebSocketState.Open)
                {
                    WebSocketReceiveResult result;
                    try
                    {
                        result = await _client
                            .ReceiveAsync(new ArraySegment<byte>(buffer), _cts.Token)
                            .ConfigureAwait(false);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    catch (WebSocketException)
                    {
                        // Server-side abrupt close surfaces as a WebSocketException; treat as a
                        // normal disconnect so the transport's reconnect path kicks in.
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
                        MessageReceived?.Invoke(text);
                    }
                }
            }
#pragma warning disable CA1031 // The receive loop runs on a worker thread; nothing meaningful to do with an unexpected exception other than surface it and exit.
            catch (Exception ex)
#pragma warning restore CA1031
            {
                ErrorOccurred?.Invoke(ex);
            }
            finally
            {
                _isAlive = false;
                Closed?.Invoke();
            }
        }

        public void Send(string text)
        {
            if (!_isAlive)
            {
                return;
            }

            var bytes = Encoding.UTF8.GetBytes(text);
            _sendSemaphore.Wait();
            try
            {
                _client
                    .SendAsync(
                        new ArraySegment<byte>(bytes),
                        WebSocketMessageType.Text,
                        endOfMessage: true,
                        _cts.Token
                    )
                    .GetAwaiter()
                    .GetResult();
            }
            finally
            {
                _sendSemaphore.Release();
            }
        }

        public void Close()
        {
            if (!_isAlive)
            {
                return;
            }

            _isAlive = false;
            try
            {
                _cts.Cancel();
            }
            catch (ObjectDisposedException) { }

            if (_client.State == WebSocketState.Open)
            {
                try
                {
                    _client
                        .CloseAsync(
                            WebSocketCloseStatus.NormalClosure,
                            "test",
                            CancellationToken.None
                        )
                        .GetAwaiter()
                        .GetResult();
                }
#pragma warning disable CA1031 // Best-effort close during teardown; nothing useful to do with a close-time exception.
                catch (Exception)
#pragma warning restore CA1031
                {
                    // ignore
                }
            }
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            Close();
            _cts.Dispose();
            _client.Dispose();
            _sendSemaphore.Dispose();
        }
    }
}
