// SPDX-License-Identifier: MIT

#region

using System;
using AwesomeAssertions;
using LobotomyCorporationMods.DontChatMe.Models;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Test.ModTests.DontChatMeTests.ModelTests
{
    public sealed class EffectReplyTests : DontChatMeModTests
    {
        [Fact]
        public void Ack_emits_dispatch_ack_with_only_the_redemption_id()
        {
            var reply = EffectReply.Ack("r-123");

            reply.ToJson().Should().Be("{\"type\":\"dispatch_ack\",\"redemption_id\":\"r-123\"}");
        }

        [Fact]
        public void Executed_emits_effect_executed_with_only_the_redemption_id()
        {
            var reply = EffectReply.Executed("r-456");

            reply
                .ToJson()
                .Should()
                .Be("{\"type\":\"effect_executed\",\"redemption_id\":\"r-456\"}");
        }

        [Fact]
        public void Failed_emits_effect_failed_with_the_error_tag()
        {
            var reply = EffectReply.Failed("r-789", "cooldown");

            reply
                .ToJson()
                .Should()
                .Be(
                    "{\"type\":\"effect_failed\",\"redemption_id\":\"r-789\",\"error\":\"cooldown\"}"
                );
        }

        [Fact]
        public void ToJson_escapes_double_quotes_in_the_redemption_id()
        {
            var reply = EffectReply.Ack("a\"b");

            reply.ToJson().Should().Be("{\"type\":\"dispatch_ack\",\"redemption_id\":\"a\\\"b\"}");
        }

        [Fact]
        public void ToJson_escapes_backslashes_and_control_characters()
        {
            var reply = EffectReply.Failed("r-1", "line1\nline2\t\\ok");

            reply
                .ToJson()
                .Should()
                .Be(
                    "{\"type\":\"effect_failed\",\"redemption_id\":\"r-1\",\"error\":\"line1\\nline2\\t\\\\ok\"}"
                );
        }

        [Fact]
        public void Ack_throws_when_redemption_id_is_null()
        {
            Action act = () => EffectReply.Ack(redemptionId: null);

            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Failed_throws_when_error_is_null()
        {
            Action act = () => EffectReply.Failed("r", error: null);

            act.Should().Throw<ArgumentNullException>();
        }
    }
}
