// SPDX-License-Identifier: MIT

#region

using AwesomeAssertions;
using LobotomyCorporationMods.DontChatMe.UiComponents;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Test.ModTests.DontChatMeTests.UiComponentTests
{
    public sealed class HudStateTests
    {
        [Fact]
        public void Initial_state_is_Disabled_when_config_is_disabled()
        {
            var state = new HudState(initiallyEnabled: false);

            state.Snapshot.State.Should().Be(ConnectionState.Disabled);
        }

        [Fact]
        public void Initial_state_is_Disconnected_when_config_is_enabled()
        {
            var state = new HudState(initiallyEnabled: true);

            state.Snapshot.State.Should().Be(ConnectionState.Disconnected);
        }

        [Fact]
        public void Initial_snapshot_is_visible_with_no_recorded_effect_and_zero_queue()
        {
            var state = new HudState(initiallyEnabled: true);

            var snapshot = state.Snapshot;
            snapshot.Visible.Should().BeTrue();
            snapshot.QueuedCount.Should().Be(0);
            snapshot.LastEffectSlug.Should().BeNull();
        }

        [Fact]
        public void SetConnectionState_updates_the_snapshot()
        {
            var state = new HudState(initiallyEnabled: true);

            state.SetConnectionState(ConnectionState.Connecting);

            state.Snapshot.State.Should().Be(ConnectionState.Connecting);
        }

        [Fact]
        public void SetQueuedCount_updates_the_snapshot()
        {
            var state = new HudState(initiallyEnabled: true);

            state.SetQueuedCount(5);

            state.Snapshot.QueuedCount.Should().Be(5);
        }

        [Fact]
        public void SetQueuedCount_clamps_negative_values_to_zero()
        {
            var state = new HudState(initiallyEnabled: true);

            state.SetQueuedCount(-3);

            state.Snapshot.QueuedCount.Should().Be(0);
        }

        [Fact]
        public void RecordEffect_updates_the_last_effect_slug()
        {
            var state = new HudState(initiallyEnabled: true);

            state.RecordEffect("add_money");

            state.Snapshot.LastEffectSlug.Should().Be("add_money");
        }

        [Fact]
        public void RecordEffect_overwrites_with_each_call()
        {
            var state = new HudState(initiallyEnabled: true);

            state.RecordEffect("add_money");
            state.RecordEffect("kill_random_agent");

            state.Snapshot.LastEffectSlug.Should().Be("kill_random_agent");
        }

        [Fact]
        public void ToggleVisibility_flips_the_visible_flag()
        {
            var state = new HudState(initiallyEnabled: true);

            state.ToggleVisibility();
            state.Snapshot.Visible.Should().BeFalse();

            state.ToggleVisibility();
            state.Snapshot.Visible.Should().BeTrue();
        }
    }
}
