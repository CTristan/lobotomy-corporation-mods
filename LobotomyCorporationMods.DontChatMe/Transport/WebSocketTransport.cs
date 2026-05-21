// SPDX-License-Identifier: MIT

#region

using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Threading;
using LobotomyCorporation.Mods.Common;
using LobotomyCorporationMods.DontChatMe.Configuration;
using LobotomyCorporationMods.DontChatMe.Constants;
using LobotomyCorporationMods.DontChatMe.Models;

#endregion

namespace LobotomyCorporationMods.DontChatMe.Transport
{
    /// <summary>
    ///     Owns the lifetime of the WebSocket connection to the chat-side server.
    ///     Sends the <c>hello</c> handshake, parses inbound frames, replies to pings, raises
    ///     <see cref="EffectReceived" /> for the dispatcher, and reconnects with exponential backoff
    ///     when the socket drops.
    ///     Thread-safe: events from the underlying socket fire on worker threads, but all state
    ///     transitions happen behind <see cref="_lock" />.
    /// </summary>
    [SuppressMessage(
        "Design",
        "CA1003",
        Justification = "Events are an assembly-private callback channel; the Action<T> form keeps subscribers terse without adding EventArgs ceremony."
    )]
    public sealed class WebSocketTransport : IDisposable
    {
        private readonly Func<Uri, IWebSocket> _webSocketFactory;
        private readonly IDontChatMeConfig _config;
        private readonly Action<Exception> _onError;
        private readonly Action<string> _info;
        private readonly Func<double> _random;
        private readonly Action<int, Action> _scheduleAfter;
        private readonly string _version;
        private readonly object _lock = new object();

        private IWebSocket _currentSocket;
        private bool _started;
        private bool _stopping;
        private int _reconnectAttempt;

        public WebSocketTransport(
            Func<Uri, IWebSocket> webSocketFactory,
            IDontChatMeConfig config,
            Action<Exception> onError,
            string version,
            Action<string> info = null,
            Func<double> random = null,
            Action<int, Action> scheduleAfter = null
        )
        {
            ThrowHelper.ThrowIfNull(webSocketFactory, nameof(webSocketFactory));
            ThrowHelper.ThrowIfNull(config, nameof(config));
            ThrowHelper.ThrowIfNull(onError, nameof(onError));
            ThrowHelper.ThrowIfNull(version, nameof(version));
            _webSocketFactory = webSocketFactory;
            _config = config;
            _onError = onError;
            _version = version;
            _info = info ?? UnityEngine.Debug.Log;
            _random = random ?? DefaultRandom;
            _scheduleAfter = scheduleAfter ?? DefaultScheduleAfter;
        }

        /// <summary>Reconnect attempt counter; exposed for diagnostics and tests.</summary>
        public int ReconnectAttempts
        {
            get
            {
                lock (_lock)
                {
                    return _reconnectAttempt;
                }
            }
        }

        /// <summary>Raised on a worker thread when the server pushes an <c>effect_dispatched</c> frame.</summary>
        public event Action<EffectDispatch> EffectReceived;

        /// <summary>Raised on a worker thread when the server returns a non-recoverable error during handshake.</summary>
        public event Action<string> ServerErrorReceived;

        /// <summary>Opens the socket. Idempotent: a second call while running is a no-op.</summary>
        public void Start()
        {
            lock (_lock)
            {
                if (_started)
                {
                    return;
                }

                _started = true;
                _stopping = false;
                _reconnectAttempt = 0;
            }

            ConnectIfPossible();
        }

        /// <summary>Closes the socket and stops the reconnect loop.</summary>
        public void Stop()
        {
            IWebSocket socket;
            lock (_lock)
            {
                _started = false;
                _stopping = true;
                socket = _currentSocket;
                _currentSocket = null;
            }

            CloseSocketSafely(socket);
            // The pending reconnect thread (if any) will see _stopping when it wakes and exit
            // without reconnecting.
        }

        /// <summary>Sends a reply frame. Best effort: drops on the floor if the socket is closed.</summary>
        public void SendReply(EffectReply reply)
        {
            ThrowHelper.ThrowIfNull(reply, nameof(reply));
            SendRaw(reply.ToJson());
        }

        public void Dispose() => Stop();

        [SuppressMessage(
            "Security",
            "CA5394",
            Justification = "Used only to jitter reconnect backoff; no security boundary."
        )]
        private static double DefaultRandom()
        {
            return new Random().NextDouble();
        }

        private void ConnectIfPossible()
        {
            if (_config.ServerUrl == null || string.IsNullOrEmpty(_config.AuthToken))
            {
                _info("DontChatMe transport not starting: ServerUrl or AuthToken is unset.");
                return;
            }

            IWebSocket socket;
            try
            {
                socket = _webSocketFactory(_config.ServerUrl);
            }
#pragma warning disable CA1031 // Any construction error is treated as a connect failure and surfaced via the reconnect path.
            catch (Exception ex)
#pragma warning restore CA1031
            {
                _onError(ex);
                ScheduleReconnect();
                return;
            }

            socket.Opened += OnOpened;
            socket.MessageReceived += OnMessageReceived;
            socket.Closed += OnClosed;
            socket.ErrorOccurred += OnErrorOccurred;

            lock (_lock)
            {
                _currentSocket = socket;
            }

            try
            {
                socket.Connect();
            }
#pragma warning disable CA1031 // A connect failure is normal and surfaces through OnClosed; we mustn't let it kill the caller.
            catch (Exception ex)
#pragma warning restore CA1031
            {
                _onError(ex);
                // OnClosed should fire from the library but in case it doesn't:
                ScheduleReconnect();
            }
        }

        private void OnOpened()
        {
            try
            {
                SendRaw(HelloFrame.Build(_config.AuthToken, _version));
                _info(LogMessages.TransportConnected);
                lock (_lock)
                {
                    _reconnectAttempt = 0;
                }
            }
#pragma warning disable CA1031 // Any unexpected exception in the handshake is logged and surfaces as a disconnect; never let it propagate.
            catch (Exception ex)
#pragma warning restore CA1031
            {
                _onError(ex);
            }
        }

        private void OnMessageReceived(string text)
        {
            HandleInbound(text);
        }

        internal void HandleInbound(string text)
        {
            if (!InboundFrame.TryParse(text, out var frame))
            {
                _info(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        LogMessages.ParseFailure,
                        text ?? "(null)"
                    )
                );
                return;
            }

            switch (frame.Type)
            {
                case WireTypes.Welcome:
                    // Welcome means server accepted our hello — nothing more to do.
                    break;

                case WireTypes.Ping:
                    SendRaw(HelloFrame.BuildPong());
                    break;

                case WireTypes.EffectDispatched:
                    if (frame.TryGetEffectDispatch(out var dispatch))
                    {
                        // Acknowledge immediately so we beat the chat-side 10s dispatch timeout
                        // even if the per-frame pump on the main thread is busy.
                        SendRaw(EffectReply.Ack(dispatch.RedemptionId).ToJson());
                        EffectReceived?.Invoke(dispatch);
                    }
                    else
                    {
                        _info(
                            string.Format(
                                CultureInfo.InvariantCulture,
                                LogMessages.ParseFailure,
                                text
                            )
                        );
                    }

                    break;

                case WireTypes.Error:
                    var code = frame.GetErrorCode() ?? "unknown";
                    _info("DontChatMe server returned error: " + code);
                    ServerErrorReceived?.Invoke(code);
                    break;

                default:
                    _info(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            LogMessages.UnknownInboundType,
                            frame.Type
                        )
                    );
                    break;
            }
        }

        private void OnClosed()
        {
            _info(LogMessages.TransportDisconnected);
            ScheduleReconnect();
        }

        private void OnErrorOccurred(Exception ex)
        {
            _onError(ex);
        }

        private void ScheduleReconnect()
        {
            int attempt;
            lock (_lock)
            {
                if (_stopping || !_started)
                {
                    return;
                }

                _reconnectAttempt++;
                attempt = _reconnectAttempt;
            }

            var delaySeconds = BackoffSchedule.DelayFor(attempt, _random());
            var delayMs = (int)(delaySeconds * 1000f);
            _info(
                string.Format(
                    CultureInfo.InvariantCulture,
                    LogMessages.TransportReconnecting,
                    delayMs,
                    attempt
                )
            );

            _scheduleAfter(
                delayMs,
                () =>
                {
                    lock (_lock)
                    {
                        if (_stopping || !_started)
                        {
                            return;
                        }
                    }

                    ConnectIfPossible();
                }
            );
        }

        private static void DefaultScheduleAfter(int delayMs, Action action)
        {
            var thread = new Thread(() =>
            {
                Thread.Sleep(delayMs);
                action();
            })
            {
                IsBackground = true,
                Name = "DontChatMe.Reconnect",
            };
            thread.Start();
        }

        private void SendRaw(string text)
        {
            IWebSocket socket;
            lock (_lock)
            {
                socket = _currentSocket;
            }

            if (socket == null || !socket.IsAlive)
            {
                return;
            }

            try
            {
                socket.Send(text);
            }
#pragma warning disable CA1031 // Send failures during a race with disconnect are expected; surface to log and continue.
            catch (Exception ex)
#pragma warning restore CA1031
            {
                _onError(ex);
            }
        }

        private static void CloseSocketSafely(IWebSocket socket)
        {
            if (socket == null)
            {
                return;
            }

            try
            {
                socket.Close();
                socket.Dispose();
            }
#pragma warning disable CA1031 // We're tearing down; the only sensible response to any close-time exception is to drop it.
            catch (Exception)
#pragma warning restore CA1031
            {
                // ignore
            }
        }
    }
}
