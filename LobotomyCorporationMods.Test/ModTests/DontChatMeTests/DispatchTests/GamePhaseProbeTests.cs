// SPDX-License-Identifier: MIT

#region

using System.Collections.Generic;
using AwesomeAssertions;
using LobotomyCorporationMods.DontChatMe.Constants;
using LobotomyCorporationMods.DontChatMe.Dispatch;
using LobotomyCorporationMods.DontChatMe.Models;
using LobotomyCorporationMods.Test.ModTests.DontChatMeTests.Fakes;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Test.ModTests.DontChatMeTests.DispatchTests
{
    public sealed class GamePhaseProbeTests : DontChatMeModTests
    {
        private sealed class Clock
        {
            public float Now { get; set; }
        }

        private static GamePhaseProbe Build(
            List<GameStateReply> sink,
            FakeGameAdapter adapter,
            Clock clock
        )
        {
            return new GamePhaseProbe(adapter: adapter, send: sink.Add, now: () => clock.Now);
        }

        [Fact]
        public void First_tick_emits_the_current_phase()
        {
            var sink = new List<GameStateReply>();
            var clock = new Clock { Now = 0f };
            var adapter = new FakeGameAdapter { Phase = GamePhases.Ready };
            var probe = Build(sink, adapter, clock);

            probe.Tick();

            sink.Should().ContainSingle();
            sink[0].Phase.Should().Be(GamePhases.Ready);
        }

        [Fact]
        public void Ticks_with_no_phase_change_stay_silent_until_the_heartbeat_fires()
        {
            var sink = new List<GameStateReply>();
            var clock = new Clock { Now = 0f };
            var adapter = new FakeGameAdapter { Phase = GamePhases.Ready };
            var probe = Build(sink, adapter, clock);

            probe.Tick(); // initial — emits
            sink.Should().ContainSingle();

            clock.Now = 1f;
            probe.Tick(); // no change, heartbeat not due
            clock.Now = 2f;
            probe.Tick();
            sink.Should().ContainSingle();

            // Heartbeat interval elapsed — emit again.
            clock.Now = GamePhaseProbe.HeartbeatIntervalSeconds + 0.01f;
            probe.Tick();
            sink.Should().HaveCount(2);
            sink[1].Phase.Should().Be(GamePhases.Ready);
        }

        [Fact]
        public void Phase_change_is_pushed_immediately_even_before_the_heartbeat_interval()
        {
            var sink = new List<GameStateReply>();
            var clock = new Clock { Now = 0f };
            var adapter = new FakeGameAdapter { Phase = GamePhases.Ready };
            var probe = Build(sink, adapter, clock);

            probe.Tick();
            sink.Should().ContainSingle();

            // Phase flips a heartbeat short of the interval.
            clock.Now = 0.2f;
            adapter.Phase = GamePhases.MeltdownActive;
            probe.Tick();

            sink.Should().HaveCount(2);
            sink[1].Phase.Should().Be(GamePhases.MeltdownActive);
        }

        [Fact]
        public void RequestSnapshot_forces_the_next_tick_to_emit_the_current_phase()
        {
            var sink = new List<GameStateReply>();
            var clock = new Clock { Now = 0f };
            var adapter = new FakeGameAdapter { Phase = GamePhases.Ready };
            var probe = Build(sink, adapter, clock);

            probe.Tick();
            sink.Should().ContainSingle();

            // No change, well before the heartbeat — snapshot should still emit.
            clock.Now = 0.2f;
            probe.RequestSnapshot();
            probe.Tick();

            sink.Should().HaveCount(2);
            sink[1].Phase.Should().Be(GamePhases.Ready);
        }

        [Fact]
        public void Tick_with_a_null_phase_emits_nothing()
        {
            var sink = new List<GameStateReply>();
            var clock = new Clock { Now = 0f };
            var adapter = new FakeGameAdapter { Phase = null };
            var probe = Build(sink, adapter, clock);

            probe.Tick();

            sink.Should().BeEmpty();
        }
    }
}
