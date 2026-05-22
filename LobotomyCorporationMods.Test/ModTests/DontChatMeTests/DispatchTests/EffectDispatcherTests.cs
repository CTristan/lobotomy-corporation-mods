// SPDX-License-Identifier: MIT

#region

using System;
using System.Collections.Generic;
using AwesomeAssertions;
using LobotomyCorporation.Mods.Common;
using LobotomyCorporationMods.DontChatMe.Configuration;
using LobotomyCorporationMods.DontChatMe.Constants;
using LobotomyCorporationMods.DontChatMe.Dispatch;
using LobotomyCorporationMods.DontChatMe.Implementations.Effects;
using LobotomyCorporationMods.DontChatMe.Models;
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
                dispatchedAt: null
            );

        private sealed class FakeConfig : IDontChatMeConfig
        {
            public Uri ServerUrl => null;
            public string AuthToken => string.Empty;
            public bool Enabled { get; set; } = true;
            public bool DangerEffectsEnabled { get; set; }
            public int MaxInFlight => 32;
            public float GlobalCooldownSeconds => 0f;
            public float EnergyAmount => 10f;
            public int MoneyAmount => 100;
        }

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
            List<EffectReply> sink,
            IEffectExecutor executor,
            FakeConfig config = null,
            CooldownGate gate = null,
            IdempotencyCache cache = null,
            Action<string> onExecuted = null
        )
        {
            return new EffectDispatcher(
                config: config ?? new FakeConfig(),
                executors: new[] { executor },
                cooldownGate: gate ?? new CooldownGate(now: () => 0f, globalCooldownSeconds: 0f),
                idempotencyCache: cache ?? new IdempotencyCache(capacity: 16),
                sendReply: sink.Add,
                logger: new Mock<ILogger>().Object,
                onExecuted: onExecuted
            );
        }

        [Fact]
        public void Dispatch_executes_a_known_slug_and_emits_effect_executed()
        {
            var sink = new List<EffectReply>();
            var executor = new FakeExecutor("add_money");
            var dispatcher = BuildDispatcher(sink, executor);

            dispatcher.Dispatch(MakeDispatch("add_money"));

            executor.InvocationCount.Should().Be(1);
            sink.Should().ContainSingle();
            sink[0].Kind.Should().Be(ReplyKind.EffectExecuted);
        }

        [Fact]
        public void Dispatch_emits_unknown_slug_when_no_executor_is_registered()
        {
            var sink = new List<EffectReply>();
            var executor = new FakeExecutor("add_money");
            var dispatcher = BuildDispatcher(sink, executor);

            dispatcher.Dispatch(MakeDispatch("not_registered"));

            executor.InvocationCount.Should().Be(0);
            sink.Should().ContainSingle();
            sink[0].Kind.Should().Be(ReplyKind.EffectFailed);
            sink[0].Error.Should().Be(ErrorTags.UnknownSlug);
        }

        [Fact]
        public void Dispatch_emits_duplicate_redemption_when_the_id_was_seen_before()
        {
            var sink = new List<EffectReply>();
            var executor = new FakeExecutor("add_money");
            var dispatcher = BuildDispatcher(sink, executor);

            dispatcher.Dispatch(MakeDispatch("add_money", "r-dup"));
            dispatcher.Dispatch(MakeDispatch("add_money", "r-dup"));

            executor.InvocationCount.Should().Be(1);
            sink.Should().HaveCount(2);
            sink[1].Kind.Should().Be(ReplyKind.EffectFailed);
            sink[1].Error.Should().Be(ErrorTags.DuplicateRedemption);
        }

        [Fact]
        public void Dispatch_blocks_a_danger_effect_when_the_config_flag_is_off()
        {
            var sink = new List<EffectReply>();
            var executor = new FakeExecutor("escape_random_creature") { IsDanger = true };
            var config = new FakeConfig { DangerEffectsEnabled = false };
            var dispatcher = BuildDispatcher(sink, executor, config: config);

            dispatcher.Dispatch(MakeDispatch("escape_random_creature"));

            executor.InvocationCount.Should().Be(0);
            sink[0].Error.Should().Be(ErrorTags.DangerEffectsDisabled);
        }

        [Fact]
        public void Dispatch_runs_a_danger_effect_when_the_config_flag_is_on()
        {
            var sink = new List<EffectReply>();
            var executor = new FakeExecutor("escape_random_creature") { IsDanger = true };
            var config = new FakeConfig { DangerEffectsEnabled = true };
            var dispatcher = BuildDispatcher(sink, executor, config: config);

            dispatcher.Dispatch(MakeDispatch("escape_random_creature"));

            executor.InvocationCount.Should().Be(1);
            sink[0].Kind.Should().Be(ReplyKind.EffectExecuted);
        }

        [Fact]
        public void Dispatch_emits_cooldown_when_the_gate_says_so()
        {
            var sink = new List<EffectReply>();
            var executor = new FakeExecutor("add_money") { CooldownSeconds = 30f };
            var now = 100f;
            var gate = new CooldownGate(now: () => now, globalCooldownSeconds: 0f);
            var dispatcher = BuildDispatcher(sink, executor, gate: gate);

            dispatcher.Dispatch(MakeDispatch("add_money", "r1"));
            dispatcher.Dispatch(MakeDispatch("add_money", "r2"));

            executor.InvocationCount.Should().Be(1);
            sink[1].Kind.Should().Be(ReplyKind.EffectFailed);
            sink[1].Error.Should().Be(ErrorTags.Cooldown);
        }

        [Fact]
        public void Dispatch_does_not_mark_cooldown_on_failed_execution()
        {
            var sink = new List<EffectReply>();
            var executor = new FakeExecutor("add_money")
            {
                CooldownSeconds = 30f,
                Behavior = _ => ErrorTags.NoAgents,
            };
            var now = 100f;
            var gate = new CooldownGate(now: () => now, globalCooldownSeconds: 0f);
            var dispatcher = BuildDispatcher(sink, executor, gate: gate);

            dispatcher.Dispatch(MakeDispatch("add_money", "r1"));
            dispatcher.Dispatch(MakeDispatch("add_money", "r2"));

            executor.InvocationCount.Should().Be(2);
            sink[0].Error.Should().Be(ErrorTags.NoAgents);
            sink[1].Error.Should().Be(ErrorTags.NoAgents);
        }

        [Fact]
        public void Dispatch_emits_execution_error_when_the_executor_throws()
        {
            var sink = new List<EffectReply>();
            var executor = new FakeExecutor("add_money")
            {
                Behavior = _ => throw new InvalidOperationException("boom"),
            };
            var dispatcher = BuildDispatcher(sink, executor);

            dispatcher.Dispatch(MakeDispatch("add_money"));

            sink[0].Kind.Should().Be(ReplyKind.EffectFailed);
            sink[0].Error.Should().Be(ErrorTags.ExecutionError);
        }

        [Fact]
        public void Dispatch_forwards_a_specific_error_tag_from_the_executor()
        {
            var sink = new List<EffectReply>();
            var executor = new FakeExecutor("kill_random_agent")
            {
                Behavior = _ => ErrorTags.NoAgents,
            };
            var dispatcher = BuildDispatcher(sink, executor);

            dispatcher.Dispatch(MakeDispatch("kill_random_agent"));

            sink[0].Kind.Should().Be(ReplyKind.EffectFailed);
            sink[0].Error.Should().Be(ErrorTags.NoAgents);
        }

        [Fact]
        public void Dispatch_invokes_the_onExecuted_callback_with_the_slug_on_success()
        {
            var sink = new List<EffectReply>();
            var executor = new FakeExecutor("add_money");
            var executed = new List<string>();
            var dispatcher = BuildDispatcher(sink, executor, onExecuted: executed.Add);

            dispatcher.Dispatch(MakeDispatch("add_money"));

            executed.Should().Equal("add_money");
        }

        [Fact]
        public void Dispatch_does_not_invoke_onExecuted_when_the_executor_throws()
        {
            var sink = new List<EffectReply>();
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
        public void Dispatch_does_not_invoke_onExecuted_when_the_executor_returns_an_error_tag()
        {
            var sink = new List<EffectReply>();
            var executor = new FakeExecutor("kill_random_agent")
            {
                Behavior = _ => ErrorTags.NoAgents,
            };
            var executed = new List<string>();
            var dispatcher = BuildDispatcher(sink, executor, onExecuted: executed.Add);

            dispatcher.Dispatch(MakeDispatch("kill_random_agent"));

            executed.Should().BeEmpty();
        }
    }
}
