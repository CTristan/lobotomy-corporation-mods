// SPDX-License-Identifier: MIT

#region

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AwesomeAssertions;
using LobotomyCorporationMods.DontChatMe.Constants;
using LobotomyCorporationMods.DontChatMe.Dispatch;
using LobotomyCorporationMods.DontChatMe.Implementations.Effects;
using LobotomyCorporationMods.DontChatMe.Models;
using LobotomyCorporationMods.DontChatMe.Transport;
using LobotomyCorporationMods.DontChatMe.UiComponents;
using LobotomyCorporationMods.Test.ModTests.DontChatMeTests.Fakes;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Test.ModTests.DontChatMeTests.IntegrationTests
{
    /// <summary>
    ///     Loopback integration tests for <see cref="WebSocketTransport" />. Exercises the full state
    ///     machine — TCP handshake, JSON wire round-trip, async event delivery, reconnect — against a
    ///     real <see cref="System.Net.WebSockets.ClientWebSocket" /> talking to an in-process
    ///     <see cref="LoopbackWebSocketServer" />. Complements the <see cref="FakeWebSocket" />-based
    ///     unit tests in <c>WebSocketTransportTests</c>, which cover the same state transitions in
    ///     isolation.
    ///     Subprotocol-based auth negotiation isn't exercised here — these tests use a raw
    ///     <see cref="ClientWebSocketAdapter" /> that does not propagate the subprotocol; the
    ///     unit tests cover that path.
    /// </summary>
    public sealed class WebSocketTransportIntegrationTests : IDisposable
    {
        private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(5);

        private readonly LoopbackWebSocketServer _server;
        private readonly FakeConfig _config;
        private readonly List<Action> _scheduledReconnects = new();
        private readonly List<EffectDispatch> _effectsReceived = new();
        private WebSocketTransport _transport;

        public WebSocketTransportIntegrationTests()
        {
            _server = new LoopbackWebSocketServer();
            _server.Start();
            _config = new FakeConfig
            {
                ServerUrl = _server.Url,
                AuthToken = "tok-int",
                GameId = 42,
            };
        }

        public void Dispose()
        {
            try
            {
                _transport?.Dispose();
            }
#pragma warning disable CA1031 // Best-effort teardown of the transport before disposing the server.
            catch (Exception)
#pragma warning restore CA1031
            {
                // ignore
            }

            _server.Dispose();
        }

        private WebSocketTransport BuildTransport(Func<string> lastSeenRedemptionIdProvider = null)
        {
            var transport = new WebSocketTransport(
                webSocketFactory: (uri, _) => new ClientWebSocketAdapter(uri),
                config: _config,
                onError: _ => { },
                version: "1.0.0",
                lastSeenRedemptionIdProvider: lastSeenRedemptionIdProvider ?? (() => null),
                info: _ => { },
                random: () => 0,
                scheduleAfter: (_, action) => _scheduledReconnects.Add(action)
            );
            transport.EffectReceived += dispatch => _effectsReceived.Add(dispatch);
            return transport;
        }

        [Fact]
        public async Task Full_lifecycle_handshake_through_success_response_round_trips_over_a_real_socket()
        {
            _transport = BuildTransport();
            _transport.Start();

            // 1. The transport opens the socket and sends a hello frame on a worker thread.
            var hello = await _server
                .WaitForFrameAsync(
                    f => f.Contains("\"type\":\"hello\"", StringComparison.Ordinal),
                    DefaultTimeout
                )
                .ConfigureAwait(true);
            hello.Should().Contain("\"game_id\":42");
            hello.Should().Contain("\"client_version\":\"1.0.0\"");
            // The auth token never appears in the frame body — it rides in the subprotocol header.
            hello.Should().NotContain("tok-int");

            // 2. Server welcomes the client. Transport transitions to Connected.
            await _server
                .PushAsync("{\"type\":\"welcome\",\"id\":1,\"queue_depth\":0}")
                .ConfigureAwait(true);

            // 3. Server pushes an effect dispatch.
            await _server
                .PushAsync(
                    "{\"type\":\"effect_dispatch\","
                        + "\"id\":2,"
                        + "\"redemption_id\":\"r-int-1\","
                        + "\"effect_slug\":\"add_money\","
                        + "\"effect_name\":null,"
                        + "\"user_id\":null,"
                        + "\"user_display_name\":null,"
                        + "\"game_id\":42,"
                        + "\"dispatched_at\":null,"
                        + "\"attempts\":1,"
                        + "\"replay\":false}"
                )
                .ConfigureAwait(true);

            // 4. EffectReceived fires on the receive-loop worker thread; the production dispatcher
            // would drain it via RequestPump on the main thread. Here we just confirm the handoff.
            await LoopbackWebSocketServer
                .WaitUntilAsync(() => _effectsReceived.Count == 1, DefaultTimeout)
                .ConfigureAwait(true);
            _effectsReceived[0].RedemptionId.Should().Be("r-int-1");
            _effectsReceived[0].EffectSlug.Should().Be("add_money");

            // 5. Test plays the role of the main-thread dispatcher and emits a single success response.
            _transport.SendResponse(EffectResponse.Success("r-int-1"));

            var response = await _server
                .WaitForFrameAsync(
                    f =>
                        f.Contains("\"type\":\"effect_response\"", StringComparison.Ordinal)
                        && f.Contains("\"redemption_id\":\"r-int-1\"", StringComparison.Ordinal)
                        && f.Contains("\"status\":\"success\"", StringComparison.Ordinal),
                    DefaultTimeout
                )
                .ConfigureAwait(true);
            response.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task Inbound_keep_alive_round_trips_a_keep_alive_back_to_the_server()
        {
            _transport = BuildTransport();
            _transport.Start();
            await _server
                .WaitForFrameAsync(
                    f => f.Contains("\"type\":\"hello\"", StringComparison.Ordinal),
                    DefaultTimeout
                )
                .ConfigureAwait(true);

            await _server.PushAsync("{\"type\":\"keep_alive\",\"id\":50}").ConfigureAwait(true);

            await _server
                .WaitForFrameAsync(
                    f =>
                        f.Contains("\"type\":\"keep_alive\"", StringComparison.Ordinal)
                        && f.Contains("\"id\":", StringComparison.Ordinal),
                    DefaultTimeout
                )
                .ConfigureAwait(true);
        }

        [Fact]
        public async Task Hello_includes_last_seen_redemption_id_when_the_provider_returns_one()
        {
            _transport = BuildTransport(lastSeenRedemptionIdProvider: () => "r-prev");
            _transport.Start();

            var hello = await _server
                .WaitForFrameAsync(
                    f => f.Contains("\"type\":\"hello\"", StringComparison.Ordinal),
                    DefaultTimeout
                )
                .ConfigureAwait(true);

            hello.Should().Contain("\"last_seen_redemption_id\":\"r-prev\"");
        }

        [Fact]
        public async Task Reconnect_after_server_close_recovers_and_processes_a_subsequent_dispatch()
        {
            _transport = BuildTransport();
            _transport.Start();

            await _server
                .WaitForFrameAsync(
                    f => f.Contains("\"type\":\"hello\"", StringComparison.Ordinal),
                    DefaultTimeout
                )
                .ConfigureAwait(true);
            await _server
                .PushAsync("{\"type\":\"welcome\",\"id\":1,\"queue_depth\":0}")
                .ConfigureAwait(true);

            // Discard the keep-alive heartbeat OnOpened scheduled; we want to isolate the
            // reconnect callback added by SimulateClose / OnClose below.
            _scheduledReconnects.Clear();

            // 1. Server initiates a close. Models any disconnect; the transport reconnects the
            // same way regardless of whether the close was orderly or a network drop.
            await _server.CloseLatestAsync().ConfigureAwait(true);

            // 2. Transport's worker thread observes the drop and queues a reconnect.
            await LoopbackWebSocketServer
                .WaitUntilAsync(() => _transport.ReconnectAttempts == 1, DefaultTimeout)
                .ConfigureAwait(true);
            _scheduledReconnects.Should().NotBeEmpty();

            // 3. Fire the scheduled reconnect immediately (test owns the scheduler).
            _scheduledReconnects[0]();

            // 4. A second hello arrives once the new socket completes its handshake.
            await LoopbackWebSocketServer
                .WaitUntilAsync(() => CountHelloFrames() >= 2, DefaultTimeout)
                .ConfigureAwait(true);
            await LoopbackWebSocketServer
                .WaitUntilAsync(() => _server.AcceptedSocketCount >= 2, DefaultTimeout)
                .ConfigureAwait(true);

            // 5. Server welcomes the new socket and pushes another dispatch.
            await _server
                .PushAsync("{\"type\":\"welcome\",\"id\":1,\"queue_depth\":0}")
                .ConfigureAwait(true);
            await _server
                .PushAsync(
                    "{\"type\":\"effect_dispatch\","
                        + "\"redemption_id\":\"r-int-2\","
                        + "\"effect_slug\":\"add_money\","
                        + "\"attempts\":1,"
                        + "\"replay\":false}"
                )
                .ConfigureAwait(true);

            await LoopbackWebSocketServer
                .WaitUntilAsync(
                    () =>
                        _effectsReceived.Count == 1
                        && _effectsReceived[0].RedemptionId == "r-int-2",
                    DefaultTimeout
                )
                .ConfigureAwait(true);

            // 6. Reconnect counter resets after the successful re-handshake (OnOpened).
            _transport.ReconnectAttempts.Should().Be(0);
        }

        private int CountHelloFrames()
        {
            var count = 0;
            foreach (var frame in _server.ReceivedFrames)
            {
                if (frame.Contains("\"type\":\"hello\"", StringComparison.Ordinal))
                {
                    count++;
                }
            }

            return count;
        }

        [Fact]
        public async Task On_welcome_the_probe_pushes_an_effect_state_snapshot_for_every_slug()
        {
            _transport = BuildTransport();
            var clock = new MutableClock();
            var probe = new AvailabilityProbe(
                executors: new IEffectExecutor[]
                {
                    new StubExecutor("alpha", available: true),
                    new StubExecutor(
                        "beta",
                        available: false,
                        reason: StandardErrors.EffectUnavailableNow
                    ),
                },
                config: _config,
                send: _transport.SendEffectState,
                now: () => clock.Now
            );
            _transport.StateChanged += state =>
            {
                if (state == ConnectionState.Connected)
                {
                    probe.RequestSnapshot();
                }
            };

            _transport.Start();

            await _server
                .WaitForFrameAsync(
                    f => f.Contains("\"type\":\"hello\"", StringComparison.Ordinal),
                    DefaultTimeout
                )
                .ConfigureAwait(true);
            await _server
                .PushAsync("{\"type\":\"welcome\",\"id\":1,\"queue_depth\":0}")
                .ConfigureAwait(true);

            // 1. The transport runs StateChanged on a worker thread; give it a beat to land before
            // we tick the probe.
            await LoopbackWebSocketServer
                .WaitUntilAsync(() => probe != null, TimeSpan.FromMilliseconds(50))
                .ConfigureAwait(true);

            // 2. Tick the probe — in production this happens from the main-thread per-frame Postfix.
            probe.Tick();

            // 3. Both effect_state frames should round-trip to the loopback server, using the new
            // `available` field (replaced the old `selectable`).
            await _server
                .WaitForFrameAsync(
                    f =>
                        f.Contains("\"type\":\"effect_state\"", StringComparison.Ordinal)
                        && f.Contains("\"effect_slug\":\"alpha\"", StringComparison.Ordinal)
                        && f.Contains("\"available\":true", StringComparison.Ordinal),
                    DefaultTimeout
                )
                .ConfigureAwait(true);

            var beta = await _server
                .WaitForFrameAsync(
                    f =>
                        f.Contains("\"type\":\"effect_state\"", StringComparison.Ordinal)
                        && f.Contains("\"effect_slug\":\"beta\"", StringComparison.Ordinal),
                    DefaultTimeout
                )
                .ConfigureAwait(true);
            beta.Should().Contain("\"available\":false");
            beta.Should().Contain("\"reason\":\"effect_unavailable_now\"");
        }

        private sealed class MutableClock
        {
            public float Now { get; set; }
        }

        private sealed class StubExecutor : IEffectExecutor
        {
            private readonly bool _available;
            private readonly string _reason;

            public StubExecutor(string slug, bool available, string reason = null)
            {
                Slug = slug;
                _available = available;
                _reason = reason;
            }

            public string Slug { get; }
            public float CooldownSeconds => 0f;
            public bool IsDanger => false;

            public string Execute(EffectDispatch dispatch) => null;

            public bool IsAvailableNow(out string reason)
            {
                reason = _available ? null : _reason;
                return _available;
            }
        }
    }
}
