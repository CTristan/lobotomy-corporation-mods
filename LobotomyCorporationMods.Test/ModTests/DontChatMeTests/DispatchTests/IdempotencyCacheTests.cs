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
        public void Add_returns_true_for_a_new_id_and_false_on_repeat()
        {
            var cache = new IdempotencyCache(capacity: 10);

            cache.Add("r1").Should().BeTrue();
            cache.Add("r1").Should().BeFalse();
        }

        [Fact]
        public void Contains_reflects_what_has_been_added()
        {
            var cache = new IdempotencyCache(capacity: 10);

            cache.Contains("r1").Should().BeFalse();
            cache.Add("r1");
            cache.Contains("r1").Should().BeTrue();
        }

        [Fact]
        public void Capacity_evicts_the_oldest_entry()
        {
            var cache = new IdempotencyCache(capacity: 2);

            cache.Add("r1").Should().BeTrue();
            cache.Add("r2").Should().BeTrue();
            cache.Add("r3").Should().BeTrue();

            cache.Contains("r1").Should().BeFalse();
            cache.Contains("r2").Should().BeTrue();
            cache.Contains("r3").Should().BeTrue();
        }

        [Fact]
        public void Re_adding_an_entry_refreshes_its_LRU_position()
        {
            var cache = new IdempotencyCache(capacity: 2);

            cache.Add("r1");
            cache.Add("r2");
            cache.Add("r1"); // touch r1 to keep it
            cache.Add("r3"); // should evict r2, not r1

            cache.Contains("r1").Should().BeTrue();
            cache.Contains("r2").Should().BeFalse();
            cache.Contains("r3").Should().BeTrue();
        }
    }
}
