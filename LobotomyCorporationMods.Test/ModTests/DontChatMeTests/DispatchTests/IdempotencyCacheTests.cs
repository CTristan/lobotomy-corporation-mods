// SPDX-License-Identifier: MIT

#region

using AwesomeAssertions;
using LobotomyCorporationMods.DontChatMe.Dispatch;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Test.ModTests.DontChatMeTests.DispatchTests
{
    public sealed class IdempotencyCacheTests : DontChatMeModTests
    {
        [Fact]
        public void TryEnter_returns_true_on_first_sight_and_marks_the_id_known()
        {
            var cache = new IdempotencyCache(capacity: 10);

            cache.TryEnter("r1").Should().BeTrue();
            cache.Contains("r1").Should().BeTrue();
            cache.IsTerminal("r1").Should().BeFalse();
        }

        [Fact]
        public void TryEnter_returns_true_after_a_retry_so_resubmits_are_allowed()
        {
            var cache = new IdempotencyCache(capacity: 10);
            cache.TryEnter("r1");
            cache.RecordRetry("r1");

            cache.TryEnter("r1").Should().BeTrue();
        }

        [Fact]
        public void TryEnter_returns_false_after_mark_terminal_so_duplicates_are_rejected()
        {
            var cache = new IdempotencyCache(capacity: 10);
            cache.TryEnter("r1");
            cache.MarkTerminal("r1");

            cache.TryEnter("r1").Should().BeFalse();
            cache.IsTerminal("r1").Should().BeTrue();
        }

        [Fact]
        public void RecordRetry_increments_and_returns_the_new_count()
        {
            var cache = new IdempotencyCache(capacity: 10);

            cache.RecordRetry("r1").Should().Be(1);
            cache.RecordRetry("r1").Should().Be(2);
            cache.RecordRetry("r1").Should().Be(3);
        }

        [Fact]
        public void RecordRetry_works_on_an_id_that_was_never_entered()
        {
            var cache = new IdempotencyCache(capacity: 10);

            cache.RecordRetry("brand_new").Should().Be(1);
            cache.Contains("brand_new").Should().BeTrue();
        }

        [Fact]
        public void MarkTerminal_works_on_an_id_that_was_never_entered()
        {
            var cache = new IdempotencyCache(capacity: 10);

            cache.MarkTerminal("brand_new");

            cache.IsTerminal("brand_new").Should().BeTrue();
            cache.TryEnter("brand_new").Should().BeFalse();
        }

        [Fact]
        public void Capacity_evicts_the_oldest_entry()
        {
            var cache = new IdempotencyCache(capacity: 2);

            cache.TryEnter("r1");
            cache.TryEnter("r2");
            cache.TryEnter("r3");

            cache.Contains("r1").Should().BeFalse();
            cache.Contains("r2").Should().BeTrue();
            cache.Contains("r3").Should().BeTrue();
        }

        [Fact]
        public void TryEnter_refreshes_LRU_position_so_an_active_id_survives_eviction()
        {
            var cache = new IdempotencyCache(capacity: 2);

            cache.TryEnter("r1");
            cache.TryEnter("r2");
            cache.TryEnter("r1"); // touch r1
            cache.TryEnter("r3"); // should evict r2, not r1

            cache.Contains("r1").Should().BeTrue();
            cache.Contains("r2").Should().BeFalse();
            cache.Contains("r3").Should().BeTrue();
        }

        [Fact]
        public void RecordRetry_refreshes_LRU_position_so_a_retrying_id_survives_eviction()
        {
            var cache = new IdempotencyCache(capacity: 2);

            cache.TryEnter("r1");
            cache.TryEnter("r2");
            cache.RecordRetry("r1"); // touch r1 via retry
            cache.TryEnter("r3"); // should evict r2, not r1

            cache.Contains("r1").Should().BeTrue();
            cache.Contains("r2").Should().BeFalse();
        }
    }
}
