// SPDX-License-Identifier: MIT

#region

using AwesomeAssertions;
using LobotomyCorporationMods.DontChatMe.Constants;
using LobotomyCorporationMods.DontChatMe.Models;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Test.ModTests.DontChatMeTests.ModelTests
{
    public sealed class InboundFrameTests : DontChatMeModTests
    {
        [Fact]
        public void TryParse_round_trips_the_full_hemograce_effect_dispatched_payload()
        {
            const string Json =
                "{"
                + "\"type\":\"effect_dispatched\","
                + "\"redemption_id\":\"abc-123\","
                + "\"effect_slug\":\"random_meltdown\","
                + "\"effect_name\":\"Random Meltdown\","
                + "\"user_id\":\"u42\","
                + "\"user_display_name\":\"Bob\","
                + "\"game_id\":7,"
                + "\"dispatched_at\":\"2026-05-21T12:00:00Z\""
                + "}";

            InboundFrame.TryParse(Json, out var frame).Should().BeTrue();
            frame.Type.Should().Be(WireTypes.EffectDispatched);

            frame.TryGetEffectDispatch(out var dispatch).Should().BeTrue();
            dispatch.RedemptionId.Should().Be("abc-123");
            dispatch.EffectSlug.Should().Be("random_meltdown");
            dispatch.EffectName.Should().Be("Random Meltdown");
            dispatch.UserId.Should().Be("u42");
            dispatch.UserDisplayName.Should().Be("Bob");
            dispatch.GameId.Should().Be(7);
            dispatch.DispatchedAt.Should().Be("2026-05-21T12:00:00Z");
        }

        [Fact]
        public void TryParse_tolerates_null_optional_fields_on_effect_dispatched()
        {
            const string Json =
                "{"
                + "\"type\":\"effect_dispatched\","
                + "\"redemption_id\":\"r-1\","
                + "\"effect_slug\":\"add_money\","
                + "\"effect_name\":null,"
                + "\"user_id\":null,"
                + "\"user_display_name\":null,"
                + "\"game_id\":1,"
                + "\"dispatched_at\":null"
                + "}";

            InboundFrame.TryParse(Json, out var frame).Should().BeTrue();
            frame.TryGetEffectDispatch(out var dispatch).Should().BeTrue();
            dispatch.EffectName.Should().BeNull();
            dispatch.UserId.Should().BeNull();
            dispatch.UserDisplayName.Should().BeNull();
            dispatch.DispatchedAt.Should().BeNull();
        }

        [Fact]
        public void TryParse_fails_on_malformed_json()
        {
            InboundFrame.TryParse("{bogus", out var frame).Should().BeFalse();
            frame.Should().BeNull();
        }

        [Fact]
        public void TryParse_fails_when_type_field_is_missing()
        {
            InboundFrame.TryParse("{\"redemption_id\":\"r\"}", out var frame).Should().BeFalse();
            frame.Should().BeNull();
        }

        [Fact]
        public void TryGetEffectDispatch_fails_when_type_is_not_effect_dispatched()
        {
            InboundFrame.TryParse("{\"type\":\"ping\"}", out var frame).Should().BeTrue();
            frame.Type.Should().Be(WireTypes.Ping);
            frame.TryGetEffectDispatch(out var dispatch).Should().BeFalse();
            dispatch.Should().BeNull();
        }

        [Fact]
        public void TryGetEffectDispatch_fails_when_required_fields_are_missing()
        {
            // No redemption_id
            InboundFrame
                .TryParse(
                    "{\"type\":\"effect_dispatched\",\"effect_slug\":\"add_money\"}",
                    out var frame
                )
                .Should()
                .BeTrue();
            frame.TryGetEffectDispatch(out var dispatch).Should().BeFalse();
            dispatch.Should().BeNull();
        }

        [Fact]
        public void GetErrorCode_and_GetErrorMessage_read_the_error_frame_fields()
        {
            const string Json =
                "{\"type\":\"error\",\"code\":\"unauthorized\",\"message\":\"bad token\"}";

            InboundFrame.TryParse(Json, out var frame).Should().BeTrue();
            frame.Type.Should().Be(WireTypes.Error);
            frame.GetErrorCode().Should().Be("unauthorized");
            frame.GetErrorMessage().Should().Be("bad token");
        }
    }
}
