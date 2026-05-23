// SPDX-License-Identifier: MIT

#region

using AwesomeAssertions;
using LobotomyCorporationMods.DontChatMe.Dispatch;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Test.ModTests.DontChatMeTests.DispatchTests
{
    public sealed class CooldownGateTests : DontChatMeModTests
    {
        [Fact]
        public void IsOnCooldown_returns_false_for_a_fresh_gate()
        {
            var gate = new CooldownGate(now: () => 0f, globalCooldownSeconds: 1f);

            gate.IsOnCooldown("any", perSlugCooldownSeconds: 5f).Should().BeFalse();
        }

        [Fact]
        public void Per_slug_cooldown_blocks_a_repeat_of_the_same_slug_within_the_window()
        {
            var now = 100f;
            var gate = new CooldownGate(now: () => now, globalCooldownSeconds: 0f);

            gate.Mark("kill_random_agent");
            now += 4f;

            gate.IsOnCooldown("kill_random_agent", perSlugCooldownSeconds: 10f).Should().BeTrue();
        }

        [Fact]
        public void Per_slug_cooldown_lifts_after_the_window_elapses()
        {
            var now = 100f;
            var gate = new CooldownGate(now: () => now, globalCooldownSeconds: 0f);

            gate.Mark("kill_random_agent");
            now += 11f;

            gate.IsOnCooldown("kill_random_agent", perSlugCooldownSeconds: 10f).Should().BeFalse();
        }

        [Fact]
        public void Per_slug_cooldown_does_not_block_a_different_slug()
        {
            var gate = new CooldownGate(now: () => 0f, globalCooldownSeconds: 0f);

            gate.Mark("kill_random_agent");

            gate.IsOnCooldown("add_energy", perSlugCooldownSeconds: 10f).Should().BeFalse();
        }

        [Fact]
        public void Global_cooldown_blocks_a_different_slug_within_the_window()
        {
            var now = 100f;
            var gate = new CooldownGate(now: () => now, globalCooldownSeconds: 5f);

            gate.Mark("kill_random_agent");
            now += 2f;

            gate.IsOnCooldown("add_energy", perSlugCooldownSeconds: 0f).Should().BeTrue();
        }

        [Fact]
        public void Global_cooldown_lifts_after_the_window_elapses()
        {
            var now = 100f;
            var gate = new CooldownGate(now: () => now, globalCooldownSeconds: 5f);

            gate.Mark("kill_random_agent");
            now += 6f;

            gate.IsOnCooldown("add_energy", perSlugCooldownSeconds: 0f).Should().BeFalse();
        }

        [Fact]
        public void Per_slug_cooldown_of_zero_is_treated_as_disabled()
        {
            var gate = new CooldownGate(now: () => 0f, globalCooldownSeconds: 0f);

            gate.Mark("add_energy");

            gate.IsOnCooldown("add_energy", perSlugCooldownSeconds: 0f).Should().BeFalse();
        }
    }
}
