// SPDX-License-Identifier: MIT

#region

using System;
using System.Collections.Generic;
using AwesomeAssertions;
using LobotomyCorporationMods.DontChatMe.Dispatch;
using LobotomyCorporationMods.DontChatMe.Models;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Test.ModTests.DontChatMeTests.DispatchTests
{
    public sealed class RequestPumpTests : DontChatMeModTests
    {
        private static EffectDispatch MakeDispatch(string id) =>
            new EffectDispatch(
                redemptionId: id,
                effectSlug: "add_money",
                effectName: null,
                userId: null,
                userDisplayName: null,
                gameId: 0,
                dispatchedAt: null,
                attempts: 1,
                replay: false
            );

        [Fact]
        public void Tick_drains_a_single_pending_dispatch()
        {
            var drained = new List<string>();
            var pump = new RequestPump(drainCallback: d => drained.Add(d.RedemptionId));

            pump.Enqueue(MakeDispatch("a"));
            pump.Tick().Should().BeTrue();

            drained.Should().Equal("a");
            pump.HasPending.Should().BeFalse();
        }

        [Fact]
        public void Tick_on_empty_slot_returns_false_and_does_not_invoke_callback()
        {
            var invoked = false;
            var pump = new RequestPump(drainCallback: _ => invoked = true);

            pump.Tick().Should().BeFalse();
            invoked.Should().BeFalse();
        }

        [Fact]
        public void HasPending_reflects_slot_state()
        {
            var pump = new RequestPump(drainCallback: _ => { });

            pump.HasPending.Should().BeFalse();
            pump.Enqueue(MakeDispatch("a"));
            pump.HasPending.Should().BeTrue();
            pump.Tick();
            pump.HasPending.Should().BeFalse();
        }

        [Fact]
        public void Enqueue_while_pending_overwrites_and_fires_onOverwrite_with_the_displaced_dispatch()
        {
            var drained = new List<string>();
            var displaced = new List<string>();
            var pump = new RequestPump(
                drainCallback: d => drained.Add(d.RedemptionId),
                onOverwrite: d => displaced.Add(d.RedemptionId)
            );

            pump.Enqueue(MakeDispatch("first"));
            pump.Enqueue(MakeDispatch("second"));

            displaced.Should().Equal("first");

            pump.Tick().Should().BeTrue();
            drained.Should().Equal("second");
        }

        [Fact]
        public void Enqueue_first_dispatch_does_not_fire_onOverwrite()
        {
            var displaced = new List<string>();
            var pump = new RequestPump(
                drainCallback: _ => { },
                onOverwrite: d => displaced.Add(d.RedemptionId)
            );

            pump.Enqueue(MakeDispatch("first"));

            displaced.Should().BeEmpty();
        }

        [Fact]
        public void Enqueue_after_drain_does_not_fire_onOverwrite()
        {
            var displaced = new List<string>();
            var pump = new RequestPump(
                drainCallback: _ => { },
                onOverwrite: d => displaced.Add(d.RedemptionId)
            );

            pump.Enqueue(MakeDispatch("first"));
            pump.Tick();
            pump.Enqueue(MakeDispatch("second"));

            displaced.Should().BeEmpty();
        }

        [Fact]
        public void Enqueue_with_null_throws()
        {
            var pump = new RequestPump(drainCallback: _ => { });

            Action act = () => pump.Enqueue(dispatch: null);

            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Constructor_with_null_drain_callback_throws()
        {
            Action act = () => _ = new RequestPump(drainCallback: null);

            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Overwrite_without_an_onOverwrite_callback_silently_replaces()
        {
            var drained = new List<string>();
            var pump = new RequestPump(drainCallback: d => drained.Add(d.RedemptionId));

            pump.Enqueue(MakeDispatch("first"));
            pump.Enqueue(MakeDispatch("second"));

            pump.Tick().Should().BeTrue();
            drained.Should().Equal("second");
        }
    }
}
