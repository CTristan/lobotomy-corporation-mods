// SPDX-License-Identifier: MIT

#region

using AwesomeAssertions;
using LobotomyCorporationMods.DontChatMe.UiComponents;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Test.ModTests.DontChatMeTests.UiComponentTests
{
    public sealed class HudFormatterTests
    {
        [Fact]
        public void Returns_null_when_the_overlay_is_hidden()
        {
            var snapshot = new HudSnapshot(ConnectionState.Connected, 0, null, visible: false);

            HudFormatter.Format(snapshot).Should().BeNull();
        }

        [Theory]
        [InlineData(ConnectionState.Disabled, "disabled")]
        [InlineData(ConnectionState.Disconnected, "disconnected")]
        [InlineData(ConnectionState.Connecting, "connecting…")]
        [InlineData(ConnectionState.Connected, "connected")]
        public void Renders_a_label_for_each_state(ConnectionState state, string expectedLabel)
        {
            var snapshot = new HudSnapshot(state, 0, null, visible: true);

            HudFormatter.Format(snapshot).Should().Contain(expectedLabel);
        }

        [Theory]
        [InlineData(ConnectionState.Disabled, "#888888")]
        [InlineData(ConnectionState.Disconnected, "#ff5555")]
        [InlineData(ConnectionState.Connecting, "#ffcc55")]
        [InlineData(ConnectionState.Connected, "#55ff77")]
        public void Renders_a_rich_text_color_dot_for_each_state(
            ConnectionState state,
            string expectedColor
        )
        {
            var snapshot = new HudSnapshot(state, 0, null, visible: true);

            HudFormatter
                .Format(snapshot)
                .Should()
                .Contain("<color=" + expectedColor + ">●</color>");
        }

        [Fact]
        public void Hides_the_queued_count_when_zero()
        {
            var snapshot = new HudSnapshot(ConnectionState.Connected, 0, null, visible: true);

            HudFormatter.Format(snapshot).Should().NotContain("queued");
        }

        [Fact]
        public void Shows_the_queued_count_when_positive()
        {
            var snapshot = new HudSnapshot(ConnectionState.Connected, 3, null, visible: true);

            HudFormatter.Format(snapshot).Should().Contain("3 queued");
        }

        [Fact]
        public void Hides_the_last_effect_section_when_no_effect_has_run()
        {
            var snapshot = new HudSnapshot(ConnectionState.Connected, 0, null, visible: true);

            HudFormatter.Format(snapshot).Should().NotContain("last:");
        }

        [Fact]
        public void Shows_the_last_effect_slug_when_one_is_recorded()
        {
            var snapshot = new HudSnapshot(
                ConnectionState.Connected,
                0,
                "add_money",
                visible: true
            );

            HudFormatter.Format(snapshot).Should().Contain("last: add_money");
        }

        [Fact]
        public void Joins_sections_with_a_middle_dot_separator()
        {
            var snapshot = new HudSnapshot(
                ConnectionState.Connected,
                2,
                "kill_random_agent",
                visible: true
            );

            HudFormatter.Format(snapshot).Should().Contain(" · 2 queued · last: kill_random_agent");
        }

        [Fact]
        public void Includes_the_DCM_prefix()
        {
            var snapshot = new HudSnapshot(ConnectionState.Connected, 0, null, visible: true);

            HudFormatter.Format(snapshot).Should().StartWith("DCM ");
        }
    }
}
