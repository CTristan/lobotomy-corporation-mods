// SPDX-License-Identifier: MIT

#region

using System;
using System.Diagnostics.CodeAnalysis;
using LobotomyCorporation.Mods.Common;
using WebSocketSharp;

#endregion

namespace LobotomyCorporationMods.DontChatMe.Transport
{
    /// <summary>
    ///     Production <see cref="IWebSocket" /> wrapping the WebSocketSharp library.
    ///     Excluded from coverage because the inside of every method is a one-line passthrough.
    /// </summary>
    [ExcludeFromCodeCoverage(Justification = Messages.UnityCodeCoverageJustification)]
    public sealed class WebSocketSharpAdapter : IWebSocket
    {
        private readonly WebSocket _ws;
        private bool _disposed;

        public WebSocketSharpAdapter(Uri url)
        {
            ThrowHelper.ThrowIfNull(url, nameof(url));
            _ws = new WebSocket(url.AbsoluteUri);
            _ws.OnOpen += (sender, e) => Opened?.Invoke();
            _ws.OnMessage += (sender, e) =>
            {
                if (e.IsText)
                {
                    MessageReceived?.Invoke(e.Data);
                }
            };
            _ws.OnClose += (sender, e) => Closed?.Invoke();
            _ws.OnError += (sender, e) =>
                ErrorOccurred?.Invoke(e.Exception ?? new InvalidOperationException(e.Message));
        }

        public event Action Opened;
        public event Action<string> MessageReceived;
        public event Action Closed;
        public event Action<Exception> ErrorOccurred;

        public bool IsAlive => _ws.IsAlive;

        public void Connect() => _ws.Connect();

        public void Send(string text) => _ws.Send(text);

        public void Close() => _ws.Close();

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            try
            {
                _ws.Close();
            }
#pragma warning disable CA1031 // Disposal must not throw — anything the close raises is swallowed.
            catch (Exception)
#pragma warning restore CA1031
            {
                // ignore
            }

            ((IDisposable)_ws).Dispose();
        }
    }
}
