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
using LobotomyCorporationMods.DontChatMe.UiComponents;

#endregion

namespace LobotomyCorporationMods.DontChatMe.Transport
{
    /// <summary>
    ///     Owns the lifetime of the WebSocket connection to the chat-side server. Sends the
    ///     <c>hello</c> handshake with the subprotocol-negotiated identifier, parses inbound
    ///     frames, emits the periodic <c>keep_alive</c>, raises <see cref="EffectReceived" />
    ///     for the dispatcher, and reconnects with exponential backoff when the socket drops
    ///     unless the close code marks the disconnect as terminal (4001 superseded, 4401
    ///     unknown_game).
    ///     Thread-safe: events from the underlying socket fire on worker threads, but all
    ///     state transitions happen behind <see cref="_lock" />.
    /// </summary>
    [SuppressMessage(
        "Design",
        "CA1003",
        Justification = "Events are an assembly-private callback channel; the Action<T> form keeps subscribers terse without adding EventArgs ceremony."
    )]
    public sealed class WebSocketTransport : IDisposable, ITransportRestarter
    {
        /// <summary>Sub-second so we comfortably beat the server's 15s idle close.</summary>
        public const int KeepAliveIntervalMs = 5000;

        /// <summary>WebSocket close code the server uses to evict an older socket for the same game_id.</summary>
        public const ushort CloseCodeSuperseded = 4001;

        /// <summary>WebSocket close code for <c>hello.game_id</c> not resolving to a game row.</summary>
        public const ushort CloseCodeUnknownGame = 4401;

        /// <summary>Server <c>error.code</c> string that also marks the connection terminal.</summary>
        public const string UnknownGameErrorCode = "unknown_game";

        /// <summary>The <c>Sec-WebSocket-Protocol</c> prefix that carries the auth identifier.</summary>
        public const string SubprotocolPrefix = "v1.token.";

        private readonly Func<Uri, string, IWebSocket> _webSocketFactory;
        private readonly IDontChatMeConfig _config;
        private readonly Action<Exception> _onError;
        private readonly Action<string> _info;
        private readonly Func<double> _random;
        private readonly Action<int, Action> _scheduleAfter;
        private readonly Func<string> _lastSeenRedemptionIdProvider;
        private readonly string _version;
        private readonly object _lock = new object();

        private IWebSocket _currentSocket;
        private bool _started;
        private bool _stopping;
        private bool _terminal;
        private int _reconnectAttempt;
        private int _outboundId;
        private int _keepAliveGeneration;

        public WebSocketTransport(
            Func<Uri, string, IWebSocket> webSocketFactory,
            IDontChatMeConfig config,
            Action<Exception> onError,
            string version,
            Func<string> lastSeenRedemptionIdProvider,
            Action<string> info = null,
            Func<double> random = null,
            Action<int, Action> scheduleAfter = null
        )
        {
            ThrowHelper.ThrowIfNull(webSocketFactory, nameof(webSocketFactory));
            ThrowHelper.ThrowIfNull(config, nameof(config));
            ThrowHelper.ThrowIfNull(onError, nameof(onError));
            ThrowHelper.ThrowIfNull(version, nameof(version));
            ThrowHelper.ThrowIfNull(
                lastSeenRedemptionIdProvider,
                nameof(lastSeenRedemptionIdProvider)
            );
            _webSocketFactory = webSocketFactory;
            _config = config;
            _onError = onError;
            _version = version;
            _lastSeenRedemptionIdProvider = lastSeenRedemptionIdProvider;
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

        /// <summary>Raised on a worker thread when the server pushes an <c>effect_dispatch</c> frame.</summary>
        public event Action<EffectDispatch> EffectReceived;

        /// <summary>Raised on a worker thread when the server returns a non-recoverable error during handshake.</summary>
        public event Action<string> ServerErrorReceived;

        /// <summary>
        ///     Raised when the connection's lifecycle state changes. The HUD overlay subscribes
        ///     to this to show the current state to the streamer.
        /// </summary>
        public event Action<ConnectionState> StateChanged;

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
                _terminal = false;
                _reconnectAttempt = 0;
            }

            ConnectIfPossible();
        }

        /// <summary>
        ///     Stops the current socket and reconnects using the current config. When
        ///     <c>_config.Enabled</c> is false, the transport stops only.
        /// </summary>
        public void Restart()
        {
            Stop();
            if (_config.Enabled)
            {
                Start();
            }
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
                _keepAliveGeneration++;
            }

            CloseSocketSafely(socket);
        }

        /// <summary>Sends an <see cref="EffectResponse" /> frame. Drops on the floor if the socket is closed.</summary>
        public void SendResponse(EffectResponse response)
        {
            ThrowHelper.ThrowIfNull(response, nameof(response));
            SendRaw(response.ToJson(NextOutboundId()));
        }

        /// <summary>Sends an <c>effect_state</c> frame. Drops on the floor if the socket is closed.</summary>
        public void SendEffectState(EffectStateReply state)
        {
            ThrowHelper.ThrowIfNull(state, nameof(state));
            SendRaw(state.ToJson(NextOutboundId()));
        }

        /// <summary>Sends a <c>game_state</c> frame. Drops on the floor if the socket is closed.</summary>
        public void SendGameState(GameStateReply gameState)
        {
            ThrowHelper.ThrowIfNull(gameState, nameof(gameState));
            SendRaw(gameState.ToJson(NextOutboundId()));
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

        private int NextOutboundId() => Interlocked.Increment(ref _outboundId);

        private void ConnectIfPossible()
        {
            if (
                _config.ServerUrl == null
                || string.IsNullOrEmpty(_config.AuthToken)
                || _config.GameId <= 0
            )
            {
                _info(
                    "DontChatMe transport not starting: ServerUrl, AuthToken, or GameId is unset."
                );
                RaiseStateChanged(ConnectionState.Disconnected);
                return;
            }

            RaiseStateChanged(ConnectionState.Connecting);

            IWebSocket socket;
            try
            {
                socket = _webSocketFactory(
                    _config.ServerUrl,
                    SubprotocolPrefix + _config.AuthToken
                );
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
#pragma warning disable CA1031 // A connect failure is normal and surfaces through OnClosed.
            catch (Exception ex)
#pragma warning restore CA1031
            {
                _onError(ex);
                ScheduleReconnect();
            }
        }

        private void OnOpened()
        {
            try
            {
                SendRaw(
                    HelloFrame.Build(
                        NextOutboundId(),
                        _config.GameId,
                        _version,
                        _lastSeenRedemptionIdProvider()
                    )
                );
                _info(LogMessages.TransportConnected);
                int generation;
                lock (_lock)
                {
                    _reconnectAttempt = 0;
                    generation = ++_keepAliveGeneration;
                }

                ScheduleKeepAlive(generation);
            }
#pragma warning disable CA1031 // Handshake exceptions are logged and surface as a disconnect; never propagate.
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
                    RaiseStateChanged(ConnectionState.Connected);
                    var depth = frame.GetWelcomeQueueDepth();
                    var inFlight = frame.GetWelcomeInFlightRedemptionId();
                    _info(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            "DontChatMe welcomed: queue_depth={0}, in_flight={1}",
                            depth,
                            inFlight ?? "(none)"
                        )
                    );
                    break;

                case WireTypes.EffectDispatch:
                    if (frame.TryGetEffectDispatch(out var dispatch))
                    {
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

                case WireTypes.KeepAlive:
                    SendKeepAlive();
                    break;

                case WireTypes.Error:
                    var code = frame.GetErrorCode() ?? "unknown";
                    _info("DontChatMe server returned error: " + code);
                    ServerErrorReceived?.Invoke(code);
                    if (string.Equals(code, UnknownGameErrorCode, StringComparison.Ordinal))
                    {
                        MarkTerminalAndStop();
                    }

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

        private void OnClosed(ushort code, string reason)
        {
            _info(
                string.Format(
                    CultureInfo.InvariantCulture,
                    "{0} (code {1} {2})",
                    LogMessages.TransportDisconnected,
                    code,
                    reason
                )
            );
            RaiseStateChanged(ConnectionState.Disconnected);

            lock (_lock)
            {
                _keepAliveGeneration++;
            }

            if (code == CloseCodeSuperseded || code == CloseCodeUnknownGame)
            {
                MarkTerminalAndStop();
                return;
            }

            ScheduleReconnect();
        }

        private void MarkTerminalAndStop()
        {
            IWebSocket socket;
            lock (_lock)
            {
                _terminal = true;
                _started = false;
                _stopping = true;
                socket = _currentSocket;
                _currentSocket = null;
                _keepAliveGeneration++;
            }

            CloseSocketSafely(socket);
        }

        private void SendKeepAlive()
        {
            var id = NextOutboundId();
            SendRaw(
                "{\""
                    + JsonKeys.Type
                    + "\":\""
                    + WireTypes.KeepAlive
                    + "\",\""
                    + JsonKeys.Id
                    + "\":"
                    + id.ToString(CultureInfo.InvariantCulture)
                    + "}"
            );
        }

        private void ScheduleKeepAlive(int generation)
        {
            _scheduleAfter(
                KeepAliveIntervalMs,
                () =>
                {
                    lock (_lock)
                    {
                        if (
                            _stopping
                            || _terminal
                            || generation != _keepAliveGeneration
                            || _currentSocket == null
                        )
                        {
                            return;
                        }
                    }

                    SendKeepAlive();
                    ScheduleKeepAlive(generation);
                }
            );
        }

        private void RaiseStateChanged(ConnectionState state)
        {
            var handler = StateChanged;
            if (handler == null)
            {
                return;
            }

            try
            {
                handler(state);
            }
#pragma warning disable CA1031 // A subscriber throwing must not tear down the transport thread.
            catch (Exception ex)
#pragma warning restore CA1031
            {
                _onError(ex);
            }
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
                if (_stopping || !_started || _terminal)
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
                        if (_stopping || !_started || _terminal)
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
                Name = "DontChatMe.Schedule",
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
