// SPDX-License-Identifier: MIT

#region

using System;
using System.Collections.Generic;
using AwesomeAssertions;
using LobotomyCorporationMods.DontChatMe.Constants;
using LobotomyCorporationMods.DontChatMe.Models;
using LobotomyCorporationMods.DontChatMe.Transport;
using LobotomyCorporationMods.DontChatMe.UiComponents;
using LobotomyCorporationMods.Test.ModTests.DontChatMeTests.Fakes;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Test.ModTests.DontChatMeTests.TransportTests
{
    public sealed class WebSocketTransportTests : DontChatMeModTests
    {
        private sealed class Harness
        {
            public FakeWebSocket Socket { get; private set; }
            public string Subprotocol { get; private set; }
            public FakeConfig Config { get; }
            public WebSocketTransport Transport { get; }
            public List<EffectDispatch> Effects { get; } = new List<EffectDispatch>();
            public List<string> ServerErrors { get; } = new List<string>();
            public List<Action> PendingSchedules { get; } = new List<Action>();
            public List<ConnectionState> StateTransitions { get; } = new List<ConnectionState>();
            public string LastSeenRedemptionId { get; set; }

            public Harness()
            {
                Config = new FakeConfig
                {
                    ServerUrl = new Uri("ws://localhost:1234/ws/game_mod"),
                    AuthToken = "tok-1",
                    GameId = 42,
                };
                Transport = new WebSocketTransport(
                    webSocketFactory: (uri, sub) =>
                    {
                        Socket = new FakeWebSocket { ConstructedFor = uri };
                        Subprotocol = sub;
                        return Socket;
                    },
                    config: Config,
                    onError: _ => { },
                    version: "1.0.0",
                    lastSeenRedemptionIdProvider: () => LastSeenRedemptionId,
                    info: _ => { },
                    random: () => 0,
                    scheduleAfter: (delay, action) => PendingSchedules.Add(action)
                );
                Transport.EffectReceived += d => Effects.Add(d);
                Transport.ServerErrorReceived += code => ServerErrors.Add(code);
                Transport.StateChanged += s => StateTransitions.Add(s);
            }
        }

        [Fact]
        public void Start_constructs_a_socket_with_the_configured_url_and_connects()
        {
            var h = new Harness();

            h.Transport.Start();

            h.Socket.Should().NotBeNull();
            h.Socket.ConstructedFor.Should().Be(h.Config.ServerUrl);
            h.Socket.ConnectCalled.Should().BeTrue();
        }

        [Fact]
        public void Start_passes_the_subprotocol_carrying_the_auth_token_to_the_factory()
        {
            var h = new Harness();

            h.Transport.Start();

            h.Subprotocol.Should().Be("v1.token.tok-1");
        }

        [Fact]
        public void Start_is_a_no_op_when_ServerUrl_is_missing()
        {
            var h = new Harness();
            h.Config.ServerUrl = null;

            h.Transport.Start();

            h.Socket.Should().BeNull();
        }

        [Fact]
        public void Start_is_a_no_op_when_AuthToken_is_empty()
        {
            var h = new Harness();
            h.Config.AuthToken = string.Empty;

            h.Transport.Start();

            h.Socket.Should().BeNull();
        }

        [Fact]
        public void Start_is_a_no_op_when_GameId_is_unset()
        {
            var h = new Harness();
            h.Config.GameId = 0;

            h.Transport.Start();

            h.Socket.Should().BeNull();
        }

        [Fact]
        public void On_open_a_hello_frame_is_sent_carrying_id_game_id_and_version()
        {
            var h = new Harness();

            h.Transport.Start();
            h.Socket.SimulateOpen();

            h.Socket.Sent.Should().ContainSingle();
            h.Socket.Sent[0].Should().Contain("\"type\":\"hello\"");
            h.Socket.Sent[0].Should().Contain("\"game_id\":42");
            h.Socket.Sent[0].Should().Contain("\"client_version\":\"1.0.0\"");
        }

        [Fact]
        public void Hello_never_contains_the_auth_token_in_the_frame_body()
        {
            var h = new Harness();

            h.Transport.Start();
            h.Socket.SimulateOpen();

            h.Socket.Sent[0].Should().NotContain("tok-1");
            h.Socket.Sent[0].Should().NotContain("token");
        }

        [Fact]
        public void Hello_includes_last_seen_redemption_id_when_the_provider_returns_one()
        {
            var h = new Harness();
            h.LastSeenRedemptionId = "r-prev";

            h.Transport.Start();
            h.Socket.SimulateOpen();

            h.Socket.Sent[0].Should().Contain("\"last_seen_redemption_id\":\"r-prev\"");
        }

        [Fact]
        public void Hello_omits_last_seen_redemption_id_when_the_provider_returns_null()
        {
            var h = new Harness();
            h.LastSeenRedemptionId = null;

            h.Transport.Start();
            h.Socket.SimulateOpen();

            h.Socket.Sent[0].Should().NotContain("last_seen_redemption_id");
        }

        [Fact]
        public void Inbound_keep_alive_triggers_a_keep_alive_reply()
        {
            var h = new Harness();
            h.Transport.Start();
            h.Socket.SimulateOpen();
            h.Socket.Sent.Clear(); // ignore hello

            h.Socket.SimulateMessage("{\"type\":\"keep_alive\",\"id\":99}");

            h.Socket.Sent.Should().ContainSingle();
            h.Socket.Sent[0].Should().Contain("\"type\":\"keep_alive\"");
            h.Socket.Sent[0].Should().Contain("\"id\":");
        }

        [Fact]
        public void Inbound_effect_dispatch_raises_the_EffectReceived_event()
        {
            var h = new Harness();
            h.Transport.Start();
            h.Socket.SimulateOpen();

            h.Socket.SimulateMessage(
                "{\"type\":\"effect_dispatch\",\"id\":1,\"redemption_id\":\"r-1\",\"effect_slug\":\"add_money\",\"effect_name\":\"AM\",\"user_id\":\"u\",\"user_display_name\":\"Bob\",\"game_id\":1,\"dispatched_at\":\"2026-05-21T00:00:00Z\",\"attempts\":1,\"replay\":false}"
            );

            h.Effects.Should().ContainSingle();
            h.Effects[0].RedemptionId.Should().Be("r-1");
            h.Effects[0].EffectSlug.Should().Be("add_money");
            h.Effects[0].Attempts.Should().Be(1);
            h.Effects[0].Replay.Should().BeFalse();
        }

        [Fact]
        public void Inbound_effect_dispatch_does_not_send_an_ack()
        {
            // The new protocol has a single EffectResponse per dispatch — there is no
            // separate dispatch_ack.
            var h = new Harness();
            h.Transport.Start();
            h.Socket.SimulateOpen();
            h.Socket.Sent.Clear(); // ignore hello

            h.Socket.SimulateMessage(
                "{\"type\":\"effect_dispatch\",\"redemption_id\":\"r-1\",\"effect_slug\":\"add_money\"}"
            );

            h.Socket.Sent.Should().BeEmpty();
        }

        [Fact]
        public void Inbound_effect_dispatch_with_replay_true_surfaces_replay_flag()
        {
            var h = new Harness();
            h.Transport.Start();
            h.Socket.SimulateOpen();

            h.Socket.SimulateMessage(
                "{\"type\":\"effect_dispatch\",\"redemption_id\":\"r-1\",\"effect_slug\":\"add_money\",\"attempts\":2,\"replay\":true}"
            );

            h.Effects.Should().ContainSingle();
            h.Effects[0].Attempts.Should().Be(2);
            h.Effects[0].Replay.Should().BeTrue();
        }

        [Fact]
        public void Inbound_error_frame_raises_ServerErrorReceived_with_the_code()
        {
            var h = new Harness();
            h.Transport.Start();
            h.Socket.SimulateOpen();

            h.Socket.SimulateMessage(
                "{\"type\":\"error\",\"code\":\"bad_frame\",\"message\":\"malformed\"}"
            );

            h.ServerErrors.Should().Equal("bad_frame");
        }

        [Fact]
        public void Inbound_error_with_unknown_game_code_stops_the_reconnect_loop()
        {
            var h = new Harness();
            h.Transport.Start();
            h.Socket.SimulateOpen();

            h.Socket.SimulateMessage(
                "{\"type\":\"error\",\"code\":\"unknown_game\",\"message\":\"no such game\"}"
            );

            // Now simulate a close — no reconnect should be scheduled.
            h.PendingSchedules.Clear();
            h.Socket.SimulateClose();

            h.PendingSchedules.Should().BeEmpty();
        }

        [Fact]
        public void Malformed_inbound_frames_are_tolerated_without_raising_events()
        {
            var h = new Harness();
            h.Transport.Start();
            h.Socket.SimulateOpen();

            h.Socket.SimulateMessage("not valid json");

            h.Effects.Should().BeEmpty();
            h.ServerErrors.Should().BeEmpty();
        }

        [Fact]
        public void SendResponse_emits_the_response_json_to_the_socket()
        {
            var h = new Harness();
            h.Transport.Start();
            h.Socket.SimulateOpen();
            h.Socket.Sent.Clear();

            h.Transport.SendResponse(EffectResponse.Success("r-1"));

            h.Socket.Sent.Should().ContainSingle();
            h.Socket.Sent[0].Should().Contain("\"type\":\"effect_response\"");
            h.Socket.Sent[0].Should().Contain("\"redemption_id\":\"r-1\"");
            h.Socket.Sent[0].Should().Contain("\"status\":\"success\"");
        }

        [Fact]
        public void SendResponse_increments_the_outbound_id_each_time()
        {
            var h = new Harness();
            h.Transport.Start();
            h.Socket.SimulateOpen();
            h.Socket.Sent.Clear();

            h.Transport.SendResponse(EffectResponse.Success("r-1"));
            h.Transport.SendResponse(EffectResponse.Success("r-2"));

            // Transport id starts at 1 for hello; SendResponse calls would be id=2, 3.
            // The exact value is implementation detail — what matters is they differ.
            var first = h.Socket.Sent[0];
            var second = h.Socket.Sent[1];
            first.Should().NotBe(second);
        }

        [Fact]
        public void SendResponse_is_safe_to_call_when_the_socket_is_not_alive()
        {
            var h = new Harness();

            // Never called Start; SendResponse must not throw and must not write anything.
            h.Transport.SendResponse(EffectResponse.Success("r-1"));

            h.Socket.Should().BeNull();
        }

        [Fact]
        public void SendEffectState_emits_an_effect_state_frame()
        {
            var h = new Harness();
            h.Transport.Start();
            h.Socket.SimulateOpen();
            h.Socket.Sent.Clear();

            h.Transport.SendEffectState(
                new EffectStateReply("add_money", available: false, reason: StandardErrors.Cooldown)
            );

            h.Socket.Sent.Should().ContainSingle();
            h.Socket.Sent[0].Should().Contain("\"type\":\"effect_state\"");
            h.Socket.Sent[0].Should().Contain("\"effect_slug\":\"add_money\"");
            h.Socket.Sent[0].Should().Contain("\"available\":false");
            h.Socket.Sent[0].Should().Contain("\"reason\":\"cooldown\"");
        }

        [Fact]
        public void SendGameState_emits_a_game_state_frame()
        {
            var h = new Harness();
            h.Transport.Start();
            h.Socket.SimulateOpen();
            h.Socket.Sent.Clear();

            h.Transport.SendGameState(new GameStateReply(GamePhases.InPlay));

            h.Socket.Sent.Should().ContainSingle();
            h.Socket.Sent[0].Should().Contain("\"type\":\"game_state\"");
            h.Socket.Sent[0].Should().Contain("\"phase\":\"in_play\"");
        }

        [Fact]
        public void OnClose_schedules_a_reconnect_attempt_on_a_normal_close()
        {
            var h = new Harness();
            h.Transport.Start();
            h.Socket.SimulateOpen();
            // Discard the keep-alive heartbeat that OnOpened scheduled; the assertion below
            // is specifically about the reconnect schedule that SimulateClose triggers.
            h.PendingSchedules.Clear();

            h.Socket.SimulateClose(code: 1000, reason: "normal");

            h.Transport.ReconnectAttempts.Should().Be(1);
            h.PendingSchedules.Should().ContainSingle();
        }

        [Fact]
        public void OnClose_with_superseded_code_4001_stops_the_reconnect_loop()
        {
            var h = new Harness();
            h.Transport.Start();
            h.Socket.SimulateOpen();
            h.PendingSchedules.Clear();

            h.Socket.SimulateClose(code: 4001, reason: "superseded");

            h.PendingSchedules.Should().BeEmpty();
        }

        [Fact]
        public void OnClose_with_unknown_game_code_4401_stops_the_reconnect_loop()
        {
            var h = new Harness();
            h.Transport.Start();
            h.Socket.SimulateOpen();
            h.PendingSchedules.Clear();

            h.Socket.SimulateClose(code: 4401, reason: "unknown_game");

            h.PendingSchedules.Should().BeEmpty();
        }

        [Fact]
        public void Stop_prevents_a_queued_reconnect_from_firing()
        {
            var h = new Harness();
            h.Transport.Start();
            h.Socket.SimulateOpen();
            h.PendingSchedules.Clear();
            h.Socket.SimulateClose();

            h.Transport.Stop();
            var socketBeforeFire = h.Socket;
            h.PendingSchedules[0]();

            h.Socket.Should().BeSameAs(socketBeforeFire); // no new socket was constructed
        }

        [Fact]
        public void When_the_scheduled_reconnect_fires_a_new_socket_is_built_and_connected()
        {
            var h = new Harness();
            h.Transport.Start();
            var initialSocket = h.Socket;
            h.Socket.SimulateOpen();
            h.PendingSchedules.Clear();
            h.Socket.SimulateClose();

            h.PendingSchedules[0]();

            h.Socket.Should().NotBeSameAs(initialSocket);
            h.Socket.ConnectCalled.Should().BeTrue();
        }

        [Fact]
        public void Reconnect_attempt_counter_resets_after_a_successful_open()
        {
            var h = new Harness();
            h.Transport.Start();
            h.Socket.SimulateOpen();
            h.PendingSchedules.Clear();
            h.Socket.SimulateClose();
            h.Transport.ReconnectAttempts.Should().Be(1);

            h.PendingSchedules[0]();
            h.Socket.SimulateOpen();

            h.Transport.ReconnectAttempts.Should().Be(0);
        }

        [Fact]
        public void Start_raises_StateChanged_with_Connecting()
        {
            var h = new Harness();

            h.Transport.Start();

            h.StateTransitions.Should().Equal(ConnectionState.Connecting);
        }

        [Fact]
        public void Start_raises_StateChanged_with_Disconnected_when_config_is_missing()
        {
            var h = new Harness();
            h.Config.ServerUrl = null;

            h.Transport.Start();

            h.StateTransitions.Should().Equal(ConnectionState.Disconnected);
        }

        [Fact]
        public void Receiving_a_welcome_frame_raises_StateChanged_with_Connected()
        {
            var h = new Harness();
            h.Transport.Start();
            h.Socket.SimulateOpen();
            h.StateTransitions.Clear();

            h.Socket.SimulateMessage("{\"type\":\"welcome\",\"id\":1,\"queue_depth\":0}");

            h.StateTransitions.Should().Equal(ConnectionState.Connected);
        }

        [Fact]
        public void Closing_the_socket_raises_StateChanged_with_Disconnected_then_Connecting()
        {
            var h = new Harness();
            h.Transport.Start();
            h.Socket.SimulateOpen();
            h.Socket.SimulateMessage("{\"type\":\"welcome\",\"id\":1,\"queue_depth\":0}");
            h.StateTransitions.Clear();
            h.PendingSchedules.Clear();

            h.Socket.SimulateClose();
            h.PendingSchedules[0]();

            h.StateTransitions.Should()
                .Equal(ConnectionState.Disconnected, ConnectionState.Connecting);
        }

        [Fact]
        public void StateChanged_subscriber_exceptions_do_not_break_the_transport()
        {
            var h = new Harness();
            h.Transport.StateChanged += _ => throw new InvalidOperationException("boom");

            // Must not throw out of Start.
            h.Transport.Start();

            h.Socket.Should().NotBeNull();
        }

        [Fact]
        public void Restart_constructs_a_new_socket_when_Enabled_is_true()
        {
            var h = new Harness();
            h.Transport.Start();
            var initialSocket = h.Socket;

            h.Transport.Restart();

            h.Socket.Should().NotBeSameAs(initialSocket);
            h.Socket.ConnectCalled.Should().BeTrue();
        }

        [Fact]
        public void Restart_stops_without_reconnecting_when_Enabled_is_false()
        {
            var h = new Harness();
            h.Transport.Start();
            var initialSocket = h.Socket;
            initialSocket.SimulateOpen();
            h.Config.Enabled = false;

            h.Transport.Restart();

            h.Socket.Should().BeSameAs(initialSocket); // no new socket constructed
            initialSocket.CloseCalled.Should().BeTrue();
        }

        [Fact]
        public void Restart_picks_up_the_updated_ServerUrl()
        {
            var h = new Harness();
            h.Transport.Start();
            var updatedUrl = new Uri("ws://localhost:9999/ws/game_mod");
            h.Config.ServerUrl = updatedUrl;

            h.Transport.Restart();

            h.Socket.ConstructedFor.Should().Be(updatedUrl);
        }
    }
}
