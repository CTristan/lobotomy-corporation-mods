// SPDX-License-Identifier: MIT

#region

using System;
using System.Collections.Generic;
using AwesomeAssertions;
using LobotomyCorporation.Mods.Common;
using LobotomyCorporationMods.DontChatMe.Constants;
using LobotomyCorporationMods.DontChatMe.Dispatch;
using LobotomyCorporationMods.DontChatMe.Implementations.Effects;
using LobotomyCorporationMods.DontChatMe.Models;
using LobotomyCorporationMods.Test.ModTests.DontChatMeTests.Fakes;
using Moq;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Test.ModTests.DontChatMeTests.DispatchTests
{
    public sealed class EffectDispatcherTests : DontChatMeModTests
    {
        private static EffectDispatch MakeDispatch(string slug, string id = "r-1") =>
            new EffectDispatch(
                redemptionId: id,
                effectSlug: slug,
                effectName: null,
                userId: null,
                userDisplayName: null,
                gameId: 0,
                dispatchedAt: null,
                attempts: 1,
                replay: false
            );

        private sealed class FakeExecutor : IEffectExecutor
        {
            public FakeExecutor(string slug)
            {
                Slug = slug;
            }

            public string Slug { get; }
            public float CooldownSeconds { get; set; }
            public bool IsDanger { get; set; }
            public Func<EffectDispatch, string> Behavior { get; set; } = _ => null;
            public int InvocationCount { get; private set; }

            public string Execute(EffectDispatch dispatch)
            {
                InvocationCount++;
                return Behavior(dispatch);
            }

            public bool IsAvailableNow(out string reason)
            {
                reason = null;
                return true;
            }
        }

        private static EffectDispatcher BuildDispatcher(
            List<EffectResponse> sink,
            IEffectExecutor executor,
            FakeConfig config = null,
            CooldownGate gate = null,
            IdempotencyCache cache = null,
            Action<string> onExecuted = null
        )
        {
            return new EffectDispatcher(
                config: config ?? new FakeConfig { DangerEffectsEnabled = true },
                executors: new[] { executor },
                cooldownGate: gate ?? new CooldownGate(now: () => 0f, globalCooldownSeconds: 0f),
                idempotencyCache: cache ?? new IdempotencyCache(capacity: 16),
                sendResponse: sink.Add,
                logger: new Mock<ILogger>().Object,
                onExecuted: onExecuted
            );
        }

        [Fact]
        public void Dispatch_runs_a_known_slug_and_emits_success()
        {
            var sink = new List<EffectResponse>();
            var executor = new FakeExecutor("add_money");
            var dispatcher = BuildDispatcher(sink, executor);

            dispatcher.Dispatch(MakeDispatch("add_money"));

            executor.InvocationCount.Should().Be(1);
            sink.Should().ContainSingle();
            sink[0].Status.Should().Be(ResponseStatuses.Success);
            sink[0].Reason.Should().BeNull();
            sink[0].RedemptionId.Should().Be("r-1");
        }

        [Fact]
        public void Dispatch_emits_effect_unknown_when_no_executor_is_registered()
        {
            var sink = new List<EffectResponse>();
            var executor = new FakeExecutor("add_money");
            var dispatcher = BuildDispatcher(sink, executor);

            dispatcher.Dispatch(MakeDispatch("not_registered"));

            executor.InvocationCount.Should().Be(0);
            sink.Should().ContainSingle();
            sink[0].Status.Should().Be(ResponseStatuses.Failure);
            sink[0].Reason.Should().Be(StandardErrors.EffectUnknown);
        }

        [Fact]
        public void Dispatch_replays_the_cached_terminal_when_the_redemption_was_already_terminated()
        {
            var sink = new List<EffectResponse>();
            var executor = new FakeExecutor("add_money");
            var dispatcher = BuildDispatcher(sink, executor);

            dispatcher.Dispatch(MakeDispatch("add_money", "r-dup"));
            dispatcher.Dispatch(MakeDispatch("add_money", "r-dup"));

            executor.InvocationCount.Should().Be(1);
            sink.Should().HaveCount(2);
            // The second emission is the cached terminal — same object, same status.
            sink[1].Status.Should().Be(ResponseStatuses.Success);
            sink[1].RedemptionId.Should().Be("r-dup");
        }

        [Fact]
        public void Dispatch_replays_the_cached_failure_when_the_redemption_terminated_as_failure()
        {
            var sink = new List<EffectResponse>();
            var executor = new FakeExecutor("kill_random_agent")
            {
                Behavior = _ => StandardErrors.EffectUnknown,
            };
            var dispatcher = BuildDispatcher(sink, executor);

            dispatcher.Dispatch(MakeDispatch("kill_random_agent", "r-fail"));
            dispatcher.Dispatch(MakeDispatch("kill_random_agent", "r-fail"));

            executor.InvocationCount.Should().Be(1);
            sink[1].Status.Should().Be(ResponseStatuses.Failure);
            sink[1].Reason.Should().Be(StandardErrors.EffectUnknown);
        }

        [Fact]
        public void Dispatch_blocks_a_danger_effect_when_the_config_flag_is_off()
        {
            var sink = new List<EffectResponse>();
            var executor = new FakeExecutor("escape_random_creature") { IsDanger = true };
            var config = new FakeConfig { DangerEffectsEnabled = false };
            var dispatcher = BuildDispatcher(sink, executor, config: config);

            dispatcher.Dispatch(MakeDispatch("escape_random_creature"));

            executor.InvocationCount.Should().Be(0);
            sink[0].Status.Should().Be(ResponseStatuses.Failure);
            sink[0].Reason.Should().Be(StandardErrors.EffectDisabled);
        }

        [Fact]
        public void Dispatch_runs_a_danger_effect_when_the_config_flag_is_on()
        {
            var sink = new List<EffectResponse>();
            var executor = new FakeExecutor("escape_random_creature") { IsDanger = true };
            var config = new FakeConfig { DangerEffectsEnabled = true };
            var dispatcher = BuildDispatcher(sink, executor, config: config);

            dispatcher.Dispatch(MakeDispatch("escape_random_creature"));

            executor.InvocationCount.Should().Be(1);
            sink[0].Status.Should().Be(ResponseStatuses.Success);
        }

        [Fact]
        public void Dispatch_emits_cooldown_failure_when_the_gate_says_so()
        {
            var sink = new List<EffectResponse>();
            var executor = new FakeExecutor("add_money") { CooldownSeconds = 30f };
            var now = 100f;
            var gate = new CooldownGate(now: () => now, globalCooldownSeconds: 0f);
            var dispatcher = BuildDispatcher(sink, executor, gate: gate);

            dispatcher.Dispatch(MakeDispatch("add_money", "r1"));
            dispatcher.Dispatch(MakeDispatch("add_money", "r2"));

            executor.InvocationCount.Should().Be(1);
            sink[1].Status.Should().Be(ResponseStatuses.Failure);
            sink[1].Reason.Should().Be(StandardErrors.Cooldown);
        }

        [Fact]
        public void Dispatch_does_not_mark_cooldown_on_failed_execution()
        {
            var sink = new List<EffectResponse>();
            var executor = new FakeExecutor("add_money")
            {
                CooldownSeconds = 30f,
                Behavior = _ => StandardErrors.EffectUnknown,
            };
            var now = 100f;
            var gate = new CooldownGate(now: () => now, globalCooldownSeconds: 0f);
            var dispatcher = BuildDispatcher(sink, executor, gate: gate);

            dispatcher.Dispatch(MakeDispatch("add_money", "r1"));
            dispatcher.Dispatch(MakeDispatch("add_money", "r2"));

            executor.InvocationCount.Should().Be(2);
            sink[0].Reason.Should().Be(StandardErrors.EffectUnknown);
            sink[1].Reason.Should().Be(StandardErrors.EffectUnknown);
        }

        [Fact]
        public void Dispatch_emits_mod_internal_error_when_the_executor_throws()
        {
            var sink = new List<EffectResponse>();
            var executor = new FakeExecutor("add_money")
            {
                Behavior = _ => throw new InvalidOperationException("boom"),
            };
            var dispatcher = BuildDispatcher(sink, executor);

            dispatcher.Dispatch(MakeDispatch("add_money"));

            sink[0].Status.Should().Be(ResponseStatuses.Failure);
            sink[0].Reason.Should().Be(StandardErrors.ModInternalError);
        }

        [Fact]
        public void Dispatch_forwards_a_specific_failure_reason_from_the_executor()
        {
            var sink = new List<EffectResponse>();
            var executor = new FakeExecutor("kill_random_agent")
            {
                Behavior = _ => StandardErrors.ViewerBlocked,
            };
            var dispatcher = BuildDispatcher(sink, executor);

            dispatcher.Dispatch(MakeDispatch("kill_random_agent"));

            sink[0].Status.Should().Be(ResponseStatuses.Failure);
            sink[0].Reason.Should().Be(StandardErrors.ViewerBlocked);
        }

        [Fact]
        public void Dispatch_invokes_the_onExecuted_callback_with_the_slug_on_success()
        {
            var sink = new List<EffectResponse>();
            var executor = new FakeExecutor("add_money");
            var executed = new List<string>();
            var dispatcher = BuildDispatcher(sink, executor, onExecuted: executed.Add);

            dispatcher.Dispatch(MakeDispatch("add_money"));

            executed.Should().Equal("add_money");
        }

        [Fact]
        public void Dispatch_does_not_invoke_onExecuted_when_the_executor_throws()
        {
            var sink = new List<EffectResponse>();
            var executor = new FakeExecutor("add_money")
            {
                Behavior = _ => throw new InvalidOperationException("boom"),
            };
            var executed = new List<string>();
            var dispatcher = BuildDispatcher(sink, executor, onExecuted: executed.Add);

            dispatcher.Dispatch(MakeDispatch("add_money"));

            executed.Should().BeEmpty();
        }

        [Fact]
        public void Dispatch_does_not_invoke_onExecuted_when_the_executor_returns_a_reason()
        {
            var sink = new List<EffectResponse>();
            var executor = new FakeExecutor("kill_random_agent")
            {
                Behavior = _ => StandardErrors.ViewerBlocked,
            };
            var executed = new List<string>();
            var dispatcher = BuildDispatcher(sink, executor, onExecuted: executed.Add);

            dispatcher.Dispatch(MakeDispatch("kill_random_agent"));

            executed.Should().BeEmpty();
        }

        // ----- retry behavior — single retry response, no cache poisoning -----

        [Fact]
        public void Dispatch_emits_retry_for_game_state_blocked_with_the_default_hold()
        {
            var sink = new List<EffectResponse>();
            var executor = new FakeExecutor("add_money")
            {
                Behavior = _ => StandardErrors.GameStateBlocked,
            };
            var dispatcher = BuildDispatcher(sink, executor);

            dispatcher.Dispatch(MakeDispatch("add_money", "r-1"));

            sink.Should().ContainSingle();
            sink[0].Status.Should().Be(ResponseStatuses.Retry);
            sink[0].Reason.Should().Be(StandardErrors.GameStateBlocked);
            sink[0].RetryAfterMs.Should().Be(EffectDispatcher.RetryHoldMs);
        }

        [Fact]
        public void Dispatch_emits_retry_for_effect_unavailable_now()
        {
            var sink = new List<EffectResponse>();
            var executor = new FakeExecutor("add_money")
            {
                Behavior = _ => StandardErrors.EffectUnavailableNow,
            };
            var dispatcher = BuildDispatcher(sink, executor);

            dispatcher.Dispatch(MakeDispatch("add_money", "r-1"));

            sink[0].Status.Should().Be(ResponseStatuses.Retry);
            sink[0].Reason.Should().Be(StandardErrors.EffectUnavailableNow);
        }

        [Fact]
        public void Dispatch_does_not_cache_a_retry_so_a_subsequent_attempt_re_executes()
        {
            // The server resends the same redemption_id on retry; the executor must run again
            // so its preconditions get re-evaluated. A cached retry would prevent that.
            var sink = new List<EffectResponse>();
            var blocked = true;
            var executor = new FakeExecutor("add_money")
            {
                Behavior = _ => blocked ? StandardErrors.GameStateBlocked : null,
            };
            var dispatcher = BuildDispatcher(sink, executor);

            dispatcher.Dispatch(MakeDispatch("add_money", "r-1"));
            blocked = false;
            dispatcher.Dispatch(MakeDispatch("add_money", "r-1"));

            executor.InvocationCount.Should().Be(2);
            sink.Should().HaveCount(2);
            sink[0].Status.Should().Be(ResponseStatuses.Retry);
            sink[1].Status.Should().Be(ResponseStatuses.Success);
        }

        [Fact]
        public void Dispatch_does_not_use_retry_for_non_transient_failures()
        {
            var sink = new List<EffectResponse>();
            var executor = new FakeExecutor("kill_random_agent")
            {
                Behavior = _ => StandardErrors.ViewerBlocked,
            };
            var dispatcher = BuildDispatcher(sink, executor);

            dispatcher.Dispatch(MakeDispatch("kill_random_agent", "r-1"));

            sink.Should().ContainSingle();
            sink[0].Status.Should().Be(ResponseStatuses.Failure);
            sink[0].Reason.Should().Be(StandardErrors.ViewerBlocked);
        }

        [Fact]
        public void Dispatch_caches_the_first_terminal_so_a_replay_after_success_is_idempotent()
        {
            var sink = new List<EffectResponse>();
            var executor = new FakeExecutor("add_money");
            var dispatcher = BuildDispatcher(sink, executor);

            dispatcher.Dispatch(MakeDispatch("add_money", "r-1"));
            dispatcher.Dispatch(MakeDispatch("add_money", "r-1"));
            dispatcher.Dispatch(MakeDispatch("add_money", "r-1"));

            executor.InvocationCount.Should().Be(1);
            sink.Should().HaveCount(3);
            sink[1].Status.Should().Be(ResponseStatuses.Success);
            sink[2].Status.Should().Be(ResponseStatuses.Success);
        }
    }
}
