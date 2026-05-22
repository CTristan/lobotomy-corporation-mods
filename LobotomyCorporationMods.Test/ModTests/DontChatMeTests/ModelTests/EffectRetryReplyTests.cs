// SPDX-License-Identifier: MIT

#region

using System;
using AwesomeAssertions;
using LobotomyCorporationMods.DontChatMe.Models;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Test.ModTests.DontChatMeTests.ModelTests
{
    public sealed class EffectRetryReplyTests : DontChatMeModTests
    {
        [Fact]
        public void ToJson_emits_effect_retry_with_redemption_id_delay_and_error()
        {
            var reply = new EffectRetryReply(
                redemptionId: "r-123",
                delayMs: 5000,
                error: "game_not_ready"
            );

            reply
                .ToJson()
                .Should()
                .Be(
                    "{\"type\":\"effect_retry\",\"redemption_id\":\"r-123\",\"delay_ms\":5000,\"error\":\"game_not_ready\"}"
                );
        }

        [Fact]
        public void ToJson_escapes_special_characters_in_string_fields()
        {
            var reply = new EffectRetryReply(redemptionId: "a\"b", delayMs: 0, error: "x\\y");

            reply
                .ToJson()
                .Should()
                .Be(
                    "{\"type\":\"effect_retry\",\"redemption_id\":\"a\\\"b\",\"delay_ms\":0,\"error\":\"x\\\\y\"}"
                );
        }

        [Fact]
        public void Negative_delay_is_clamped_to_zero()
        {
            var reply = new EffectRetryReply(redemptionId: "r-1", delayMs: -1, error: "overloaded");

            reply.DelayMs.Should().Be(0);
        }

        [Fact]
        public void Constructor_throws_when_redemption_id_is_null()
        {
            Action act = () =>
                _ = new EffectRetryReply(redemptionId: null, delayMs: 0, error: "overloaded");

            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Constructor_throws_when_error_is_null()
        {
            Action act = () => _ = new EffectRetryReply(redemptionId: "r", delayMs: 0, error: null);

            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Equals_compares_all_three_fields()
        {
            var a = new EffectRetryReply("r-1", 1000, "overloaded");
            var b = new EffectRetryReply("r-1", 1000, "overloaded");
            var c = new EffectRetryReply("r-1", 1001, "overloaded");

            a.Equals(b).Should().BeTrue();
            a.Equals(c).Should().BeFalse();
            a.GetHashCode().Should().Be(b.GetHashCode());
        }
    }
}
