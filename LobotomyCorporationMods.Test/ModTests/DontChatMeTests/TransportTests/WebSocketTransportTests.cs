// SPDX-License-Identifier: MIT

#region

using System;
using System.Collections.Generic;
using AwesomeAssertions;
using LobotomyCorporation.Mods.Common;
using LobotomyCorporationMods.DontChatMe.Models;
using LobotomyCorporationMods.DontChatMe.Transport;
using LobotomyCorporationMods.Test.ModTests.DontChatMeTests.Fakes;
using Moq;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Test.ModTests.DontChatMeTests.TransportTests
{
    public sealed class WebSocketTransportTests : DontChatMeModTests
    {
        private sealed class Harness
        {
            public FakeWebSocket Socket { get; private set; }
            public FakeConfig Config { get; }
            public WebSocketTransport Transport { get; }
            public List<EffectDispatch> Effects { get; } = new List<EffectDispatch>();
            public List<string> ServerErrors { get; } = new List<string>();
            public List<Action> PendingSchedules { get; } = new List<Action>();

            public Harness()
            {
                Config = new FakeConfig
                {
                    ServerUrl = new Uri("ws://localhost:1234/mod/socket"),
                    AuthToken = "tok-1",
                };
                Transport = new WebSocketTransport(
                    webSocketFactory: uri =>
                    {
                        Socket = new FakeWebSocket { ConstructedFor = uri };
                        return Socket;
                    },
                    config: Config,
                    onError: _ => { },
                    version: "1.0.0",
                    info: _ => { },
                    random: () => 0,
                    scheduleAfter: (delay, action) => PendingSchedules.Add(action)
                );
                Transport.EffectReceived += d => Effects.Add(d);
                Transport.ServerErrorReceived += code => ServerErrors.Add(code);
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
        public void On_open_a_hello_frame_is_sent_with_the_configured_token()
        {
            var h = new Harness();

            h.Transport.Start();
            h.Socket.SimulateOpen();

            h.Socket.Sent.Should().ContainSingle();
            h.Socket.Sent[0].Should().Contain("\"type\":\"hello\"");
            h.Socket.Sent[0].Should().Contain("\"token\":\"tok-1\"");
        }

        [Fact]
        public void Inbound_ping_triggers_a_pong_reply()
        {
            var h = new Harness();
            h.Transport.Start();
            h.Socket.SimulateOpen();
            h.Socket.Sent.Clear(); // ignore hello

            h.Socket.SimulateMessage("{\"type\":\"ping\"}");

            h.Socket.Sent.Should().ContainSingle();
            h.Socket.Sent[0].Should().Be("{\"type\":\"pong\"}");
        }

        [Fact]
        public void Inbound_effect_dispatched_raises_the_EffectReceived_event()
        {
            var h = new Harness();
            h.Transport.Start();
            h.Socket.SimulateOpen();

            h.Socket.SimulateMessage(
                "{\"type\":\"effect_dispatched\",\"redemption_id\":\"r-1\",\"effect_slug\":\"add_money\",\"effect_name\":\"AM\",\"user_id\":\"u\",\"user_display_name\":\"Bob\",\"game_id\":1,\"dispatched_at\":\"2026-05-21T00:00:00Z\"}"
            );

            h.Effects.Should().ContainSingle();
            h.Effects[0].RedemptionId.Should().Be("r-1");
            h.Effects[0].EffectSlug.Should().Be("add_money");
        }

        [Fact]
        public void Inbound_effect_dispatched_immediately_sends_a_dispatch_ack()
        {
            var h = new Harness();
            h.Transport.Start();
            h.Socket.SimulateOpen();
            h.Socket.Sent.Clear(); // ignore hello

            h.Socket.SimulateMessage(
                "{\"type\":\"effect_dispatched\",\"redemption_id\":\"r-1\",\"effect_slug\":\"add_money\",\"effect_name\":null,\"user_id\":null,\"user_display_name\":null,\"game_id\":0,\"dispatched_at\":null}"
            );

            h.Socket.Sent.Should()
                .ContainSingle(s =>
                    s.Contains("\"type\":\"dispatch_ack\"")
                    && s.Contains("\"redemption_id\":\"r-1\"")
                );
        }

        [Fact]
        public void Inbound_error_frame_raises_ServerErrorReceived_with_the_code()
        {
            var h = new Harness();
            h.Transport.Start();
            h.Socket.SimulateOpen();

            h.Socket.SimulateMessage(
                "{\"type\":\"error\",\"code\":\"unauthorized\",\"message\":\"bad token\"}"
            );

            h.ServerErrors.Should().Equal("unauthorized");
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
        public void SendReply_emits_the_reply_json_to_the_socket()
        {
            var h = new Harness();
            h.Transport.Start();
            h.Socket.SimulateOpen();
            h.Socket.Sent.Clear();

            h.Transport.SendReply(EffectReply.Executed("r-1"));

            h.Socket.Sent.Should().ContainSingle();
            h.Socket.Sent[0].Should().Be(EffectReply.Executed("r-1").ToJson());
        }

        [Fact]
        public void SendReply_is_safe_to_call_when_the_socket_is_not_alive()
        {
            var h = new Harness();

            // Never called Start; SendReply must not throw and must not write anything.
            h.Transport.SendReply(EffectReply.Executed("r-1"));

            h.Socket.Should().BeNull();
        }

        [Fact]
        public void OnClose_schedules_a_reconnect_attempt()
        {
            var h = new Harness();
            h.Transport.Start();
            h.Socket.SimulateOpen();

            h.Socket.SimulateClose();

            h.Transport.ReconnectAttempts.Should().Be(1);
            h.PendingSchedules.Should().ContainSingle();
        }

        [Fact]
        public void Stop_prevents_a_queued_reconnect_from_firing()
        {
            var h = new Harness();
            h.Transport.Start();
            h.Socket.SimulateOpen();
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
            h.Socket.SimulateClose();
            h.Transport.ReconnectAttempts.Should().Be(1);

            h.PendingSchedules[0]();
            h.Socket.SimulateOpen();

            h.Transport.ReconnectAttempts.Should().Be(0);
        }
    }
}
