// SPDX-License-Identifier: MIT

#region

using System;
using AwesomeAssertions;
using LobotomyCorporationMods.DontChatMe.Constants;
using LobotomyCorporationMods.DontChatMe.Dispatch;
using LobotomyCorporationMods.DontChatMe.Models;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Test.ModTests.DontChatMeTests.DispatchTests
{
    public sealed class IdempotencyCacheTests : DontChatMeModTests
    {
        [Fact]
        public void LastTerminal_returns_null_for_an_unknown_redemption()
        {
            var cache = new IdempotencyCache(capacity: 32);

            cache.LastTerminal("never_seen").Should().BeNull();
            cache.Count.Should().Be(0);
        }

        [Fact]
        public void MarkTerminal_stores_the_response_so_LastTerminal_returns_it()
        {
            var cache = new IdempotencyCache(capacity: 32);
            var response = EffectResponse.Success("r-1");

            cache.MarkTerminal("r-1", response);

            cache.LastTerminal("r-1").Should().BeSameAs(response);
            cache.Count.Should().Be(1);
        }

        [Fact]
        public void MarkTerminal_supports_each_terminal_status_independently()
        {
            var cache = new IdempotencyCache(capacity: 32);
            cache.MarkTerminal("r-ok", EffectResponse.Success("r-ok"));
            cache.MarkTerminal(
                "r-fail",
                EffectResponse.Failure("r-fail", StandardErrors.ViewerBlocked)
            );
            cache.MarkTerminal("r-dup", EffectResponse.Duplicate("r-dup"));

            cache.LastTerminal("r-ok").Status.Should().Be(ResponseStatuses.Success);
            cache.LastTerminal("r-fail").Status.Should().Be(ResponseStatuses.Failure);
            cache.LastTerminal("r-fail").Reason.Should().Be(StandardErrors.ViewerBlocked);
            cache.LastTerminal("r-dup").Status.Should().Be(ResponseStatuses.Duplicate);
        }

        [Fact]
        public void MarkTerminal_overwrites_a_prior_terminal_for_the_same_redemption()
        {
            var cache = new IdempotencyCache(capacity: 32);
            cache.MarkTerminal("r-1", EffectResponse.Success("r-1"));
            cache.MarkTerminal("r-1", EffectResponse.Failure("r-1", StandardErrors.Cooldown));

            cache.Count.Should().Be(1);
            cache.LastTerminal("r-1").Status.Should().Be(ResponseStatuses.Failure);
        }

        [Fact]
        public void MostRecentTerminalRedemptionId_is_null_when_the_cache_is_empty()
        {
            var cache = new IdempotencyCache(capacity: 32);

            cache.MostRecentTerminalRedemptionId.Should().BeNull();
        }

        [Fact]
        public void MostRecentTerminalRedemptionId_returns_the_most_recently_marked_redemption()
        {
            var cache = new IdempotencyCache(capacity: 32);
            cache.MarkTerminal("r-1", EffectResponse.Success("r-1"));
            cache.MarkTerminal("r-2", EffectResponse.Success("r-2"));

            cache.MostRecentTerminalRedemptionId.Should().Be("r-2");
        }

        [Fact]
        public void LastTerminal_touches_LRU_so_a_hit_becomes_the_most_recent()
        {
            var cache = new IdempotencyCache(capacity: 32);
            cache.MarkTerminal("r-1", EffectResponse.Success("r-1"));
            cache.MarkTerminal("r-2", EffectResponse.Success("r-2"));

            cache.LastTerminal("r-1");

            cache.MostRecentTerminalRedemptionId.Should().Be("r-1");
        }

        [Fact]
        public void Capacity_evicts_the_oldest_terminal_when_full()
        {
            var cache = new IdempotencyCache(capacity: 2);
            cache.MarkTerminal("r-1", EffectResponse.Success("r-1"));
            cache.MarkTerminal("r-2", EffectResponse.Success("r-2"));
            cache.MarkTerminal("r-3", EffectResponse.Success("r-3"));

            cache.LastTerminal("r-1").Should().BeNull();
            cache.LastTerminal("r-2").Should().NotBeNull();
            cache.LastTerminal("r-3").Should().NotBeNull();
            cache.Count.Should().Be(2);
        }

        [Fact]
        public void LastTerminal_touch_protects_an_active_entry_from_eviction()
        {
            var cache = new IdempotencyCache(capacity: 2);
            cache.MarkTerminal("r-1", EffectResponse.Success("r-1"));
            cache.MarkTerminal("r-2", EffectResponse.Success("r-2"));
            // touch r-1 so it's most-recent again
            cache.LastTerminal("r-1");
            cache.MarkTerminal("r-3", EffectResponse.Success("r-3"));

            cache.LastTerminal("r-1").Should().NotBeNull();
            cache.LastTerminal("r-2").Should().BeNull();
            cache.LastTerminal("r-3").Should().NotBeNull();
        }

        [Fact]
        public void Capacity_below_one_is_floored_to_one()
        {
            var cache = new IdempotencyCache(capacity: 0);

            cache.MarkTerminal("r-1", EffectResponse.Success("r-1"));
            cache.MarkTerminal("r-2", EffectResponse.Success("r-2"));

            cache.Count.Should().Be(1);
            cache.LastTerminal("r-2").Should().NotBeNull();
        }

        [Fact]
        public void LastTerminal_throws_when_redemption_id_is_null()
        {
            var cache = new IdempotencyCache(capacity: 32);

            Action act = () => cache.LastTerminal(redemptionId: null);

            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void MarkTerminal_throws_when_redemption_id_is_null()
        {
            var cache = new IdempotencyCache(capacity: 32);

            Action act = () =>
                cache.MarkTerminal(redemptionId: null, response: EffectResponse.Success("r"));

            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void MarkTerminal_throws_when_response_is_null()
        {
            var cache = new IdempotencyCache(capacity: 32);

            Action act = () => cache.MarkTerminal("r-1", response: null);

            act.Should().Throw<ArgumentNullException>();
        }
    }
}
