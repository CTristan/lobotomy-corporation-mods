// SPDX-License-Identifier: MIT

#region

using System.Collections.Generic;
using AwesomeAssertions;
using LobotomyCorporationMods.DontChatMe.Constants;
using LobotomyCorporationMods.DontChatMe.Dispatch;
using LobotomyCorporationMods.DontChatMe.Implementations.Effects;
using LobotomyCorporationMods.DontChatMe.Models;
using LobotomyCorporationMods.Test.ModTests.DontChatMeTests.Fakes;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Test.ModTests.DontChatMeTests.DispatchTests
{
    public sealed class AvailabilityProbeTests : DontChatMeModTests
    {
        private sealed class FakeExecutor : IEffectExecutor
        {
            public FakeExecutor(string slug)
            {
                Slug = slug;
            }

            public string Slug { get; }
            public float CooldownSeconds => 0f;
            public bool IsDanger { get; set; }
            public bool Available { get; set; } = true;
            public string Reason { get; set; }

            public string Execute(EffectDispatch dispatch) => null;

            public bool IsAvailableNow(out string reason)
            {
                reason = Available ? null : Reason;
                return Available;
            }
        }

        // We need a local mutable clock so tests can advance time between ticks. Capturing a ref in
        // a lambda isn't supported in C#, so we use a tiny class as a cell.
        private sealed class Clock
        {
            public float Now { get; set; }
        }

        private static AvailabilityProbe Build(
            List<EffectStateReply> sink,
            FakeConfig config,
            Clock clock,
            params IEffectExecutor[] executors
        )
        {
            return new AvailabilityProbe(
                executors: executors,
                config: config,
                send: sink.Add,
                now: () => clock.Now
            );
        }

        [Fact]
        public void Tick_without_a_snapshot_request_emits_nothing_when_no_state_has_changed_after_throttle()
        {
            var sink = new List<EffectStateReply>();
            var clock = new Clock { Now = 0f };
            var executor = new FakeExecutor("a") { Available = true };
            var probe = Build(sink, new FakeConfig(), clock, executor);

            // First tick at t=0 should still send the initial state (the probe has no prior memory).
            probe.Tick();
            sink.Should().ContainSingle();
            sink[0].Slug.Should().Be("a");
            sink[0].Available.Should().BeTrue();

            // Subsequent tick before throttle elapses: should be ignored.
            clock.Now = 0.5f;
            probe.Tick();
            sink.Should().ContainSingle();

            // After throttle elapses but state still unchanged: still no new frame.
            clock.Now = 1.1f;
            probe.Tick();
            sink.Should().ContainSingle();
        }

        [Fact]
        public void Tick_emits_a_delta_when_an_executor_flips_from_available_to_unavailable()
        {
            var sink = new List<EffectStateReply>();
            var clock = new Clock { Now = 0f };
            var executor = new FakeExecutor("kill_random_agent") { Available = true };
            var probe = Build(sink, new FakeConfig(), clock, executor);

            probe.Tick();
            sink.Should().ContainSingle();

            // Agents die — flip to unavailable.
            executor.Available = false;
            executor.Reason = StandardErrors.EffectUnavailableNow;
            clock.Now = 1.5f;
            probe.Tick();

            sink.Should().HaveCount(2);
            sink[1].Available.Should().BeFalse();
            sink[1].Reason.Should().Be(StandardErrors.EffectUnavailableNow);
        }

        [Fact]
        public void RequestSnapshot_forces_the_next_tick_to_emit_every_slug_regardless_of_change()
        {
            var sink = new List<EffectStateReply>();
            var clock = new Clock { Now = 0f };
            var probe = Build(
                sink,
                new FakeConfig(),
                clock,
                new FakeExecutor("a") { Available = true },
                new FakeExecutor("b") { Available = true }
            );

            // Initial tick lays down baseline for both slugs.
            probe.Tick();
            sink.Should().HaveCount(2);

            // Steady state — no changes between ticks.
            clock.Now = 1.5f;
            probe.Tick();
            sink.Should().HaveCount(2);

            // Snapshot fires immediately on the next tick (bypassing throttle).
            probe.RequestSnapshot();
            probe.Tick();
            sink.Should().HaveCount(4);
            sink[2].Slug.Should().Be("a");
            sink[3].Slug.Should().Be("b");
        }

        [Fact]
        public void RequestSnapshot_bypasses_the_throttle_so_the_full_state_is_emitted_immediately()
        {
            var sink = new List<EffectStateReply>();
            var clock = new Clock { Now = 0f };
            var executor = new FakeExecutor("a") { Available = true };
            var probe = Build(sink, new FakeConfig(), clock, executor);

            probe.Tick();
            sink.Should().ContainSingle();

            // 100ms later we get a snapshot request — should not wait for the 1s throttle.
            clock.Now = 0.1f;
            probe.RequestSnapshot();
            probe.Tick();
            sink.Should().HaveCount(2);
        }

        [Fact]
        public void Danger_effect_reports_unavailable_with_effect_disabled_when_the_config_flag_is_off()
        {
            var sink = new List<EffectStateReply>();
            var clock = new Clock { Now = 0f };
            var config = new FakeConfig { DangerEffectsEnabled = false };
            var executor = new FakeExecutor("escape_random_creature")
            {
                IsDanger = true,
                Available = true, // would be available on game-state grounds alone
            };
            var probe = Build(sink, config, clock, executor);

            probe.Tick();

            sink.Should().ContainSingle();
            sink[0].Available.Should().BeFalse();
            sink[0].Reason.Should().Be(StandardErrors.EffectDisabled);
        }

        [Fact]
        public void Danger_effect_defers_to_IsAvailableNow_when_the_config_flag_is_on()
        {
            var sink = new List<EffectStateReply>();
            var clock = new Clock { Now = 0f };
            var config = new FakeConfig { DangerEffectsEnabled = true };
            var executor = new FakeExecutor("escape_random_creature")
            {
                IsDanger = true,
                Available = false,
                Reason = StandardErrors.EffectUnavailableNow,
            };
            var probe = Build(sink, config, clock, executor);

            probe.Tick();

            sink.Should().ContainSingle();
            sink[0].Available.Should().BeFalse();
            sink[0].Reason.Should().Be(StandardErrors.EffectUnavailableNow);
        }

        [Fact]
        public void Probe_skips_null_executors_and_executors_with_empty_slugs()
        {
            var sink = new List<EffectStateReply>();
            var clock = new Clock { Now = 0f };
            var valid = new FakeExecutor("a") { Available = true };
            var emptySlug = new FakeExecutor(string.Empty);
            var probe = Build(sink, new FakeConfig(), clock, valid, emptySlug, null);

            probe.Tick();

            sink.Should().ContainSingle();
            sink[0].Slug.Should().Be("a");
        }
    }
}
