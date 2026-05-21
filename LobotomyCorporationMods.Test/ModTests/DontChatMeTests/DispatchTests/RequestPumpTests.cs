// SPDX-License-Identifier: MIT

#region

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
                dispatchedAt: null
            );

        [Fact]
        public void Tick_drains_in_FIFO_order()
        {
            var drained = new List<string>();
            var pump = new RequestPump(
                capacity: 10,
                maxPerTick: 10,
                drainCallback: d => drained.Add(d.RedemptionId)
            );

            pump.TryEnqueue(MakeDispatch("a")).Should().BeTrue();
            pump.TryEnqueue(MakeDispatch("b")).Should().BeTrue();
            pump.TryEnqueue(MakeDispatch("c")).Should().BeTrue();

            pump.Tick().Should().Be(3);
            drained.Should().Equal("a", "b", "c");
        }

        [Fact]
        public void TryEnqueue_returns_false_when_the_queue_is_at_capacity()
        {
            var pump = new RequestPump(capacity: 2, maxPerTick: 10, drainCallback: _ => { });

            pump.TryEnqueue(MakeDispatch("a")).Should().BeTrue();
            pump.TryEnqueue(MakeDispatch("b")).Should().BeTrue();
            pump.TryEnqueue(MakeDispatch("c")).Should().BeFalse();
            pump.Count.Should().Be(2);
        }

        [Fact]
        public void Tick_drains_at_most_maxPerTick_items_per_call()
        {
            var drained = new List<string>();
            var pump = new RequestPump(
                capacity: 10,
                maxPerTick: 2,
                drainCallback: d => drained.Add(d.RedemptionId)
            );

            pump.TryEnqueue(MakeDispatch("a"));
            pump.TryEnqueue(MakeDispatch("b"));
            pump.TryEnqueue(MakeDispatch("c"));

            pump.Tick().Should().Be(2);
            drained.Should().Equal("a", "b");
            pump.Count.Should().Be(1);

            pump.Tick().Should().Be(1);
            drained.Should().Equal("a", "b", "c");
        }

        [Fact]
        public void Tick_on_empty_queue_returns_zero()
        {
            var pump = new RequestPump(capacity: 10, maxPerTick: 10, drainCallback: _ => { });

            pump.Tick().Should().Be(0);
        }
    }
}
