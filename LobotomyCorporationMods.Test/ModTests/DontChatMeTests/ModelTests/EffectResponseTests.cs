// SPDX-License-Identifier: MIT

#region

using System;
using AwesomeAssertions;
using LobotomyCorporationMods.DontChatMe.Constants;
using LobotomyCorporationMods.DontChatMe.Models;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Test.ModTests.DontChatMeTests.ModelTests
{
    public sealed class EffectResponseTests : DontChatMeModTests
    {
        [Fact]
        public void Success_factory_records_status_and_clears_reason_and_retry()
        {
            var response = EffectResponse.Success("r-1");

            response.RedemptionId.Should().Be("r-1");
            response.Status.Should().Be(ResponseStatuses.Success);
            response.Reason.Should().BeNull();
            response.RetryAfterMs.Should().Be(EffectResponse.NoRetryAfter);
        }

        [Fact]
        public void Failure_factory_records_status_and_reason_with_no_retry()
        {
            var response = EffectResponse.Failure("r-1", StandardErrors.Cooldown);

            response.Status.Should().Be(ResponseStatuses.Failure);
            response.Reason.Should().Be(StandardErrors.Cooldown);
            response.RetryAfterMs.Should().Be(EffectResponse.NoRetryAfter);
        }

        [Fact]
        public void Retry_factory_records_status_reason_and_advisory_delay()
        {
            var response = EffectResponse.Retry("r-1", StandardErrors.GameStateBlocked, 5000);

            response.Status.Should().Be(ResponseStatuses.Retry);
            response.Reason.Should().Be(StandardErrors.GameStateBlocked);
            response.RetryAfterMs.Should().Be(5000);
        }

        [Fact]
        public void Retry_clamps_negative_retry_after_to_zero()
        {
            var response = EffectResponse.Retry("r-1", StandardErrors.GameStateBlocked, -42);

            response.RetryAfterMs.Should().Be(0);
        }

        [Fact]
        public void Duplicate_factory_records_duplicate_status_with_duplicate_reason()
        {
            var response = EffectResponse.Duplicate("r-1");

            response.Status.Should().Be(ResponseStatuses.Duplicate);
            response.Reason.Should().Be(StandardErrors.DuplicateRedemption);
            response.RetryAfterMs.Should().Be(EffectResponse.NoRetryAfter);
        }

        [Fact]
        public void ToJson_for_success_emits_envelope_without_reason_or_retry_after()
        {
            EffectResponse
                .Success("r-1")
                .ToJson(id: 7)
                .Should()
                .Be(
                    "{\"type\":\"effect_response\",\"id\":7,\"redemption_id\":\"r-1\",\"status\":\"success\"}"
                );
        }

        [Fact]
        public void ToJson_for_failure_includes_reason()
        {
            EffectResponse
                .Failure("r-2", StandardErrors.EffectUnknown)
                .ToJson(id: 8)
                .Should()
                .Be(
                    "{\"type\":\"effect_response\",\"id\":8,\"redemption_id\":\"r-2\",\"status\":\"failure\",\"reason\":\"effect_unknown\"}"
                );
        }

        [Fact]
        public void ToJson_for_retry_includes_reason_and_retry_after_ms()
        {
            EffectResponse
                .Retry("r-3", StandardErrors.GameStateBlocked, 2500)
                .ToJson(id: 9)
                .Should()
                .Be(
                    "{\"type\":\"effect_response\",\"id\":9,\"redemption_id\":\"r-3\",\"status\":\"retry\",\"reason\":\"game_state_blocked\",\"retry_after_ms\":2500}"
                );
        }

        [Fact]
        public void ToJson_for_duplicate_includes_duplicate_reason_but_no_retry_after()
        {
            EffectResponse
                .Duplicate("r-4")
                .ToJson(id: 10)
                .Should()
                .Be(
                    "{\"type\":\"effect_response\",\"id\":10,\"redemption_id\":\"r-4\",\"status\":\"duplicate\",\"reason\":\"duplicate_redemption\"}"
                );
        }

        [Fact]
        public void ToJson_escapes_special_characters_in_string_fields()
        {
            EffectResponse
                .Failure("a\"b", "line1\nline2\\tail")
                .ToJson(id: 1)
                .Should()
                .Contain("\"redemption_id\":\"a\\\"b\"")
                .And.Contain("\"reason\":\"line1\\nline2\\\\tail\"");
        }

        [Fact]
        public void ToJson_uses_invariant_culture_for_numeric_fields()
        {
            EffectResponse
                .Retry("r", StandardErrors.GameStateBlocked, 1234)
                .ToJson(id: 5678)
                .Should()
                .Contain("\"id\":5678")
                .And.Contain("\"retry_after_ms\":1234");
        }

        [Fact]
        public void Success_throws_when_redemption_id_is_null()
        {
            Action act = () => EffectResponse.Success(redemptionId: null);

            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Failure_throws_when_redemption_id_is_null()
        {
            Action act = () =>
                EffectResponse.Failure(redemptionId: null, reason: StandardErrors.Cooldown);

            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Failure_throws_when_reason_is_null()
        {
            Action act = () => EffectResponse.Failure("r-1", reason: null);

            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Retry_throws_when_reason_is_null()
        {
            Action act = () =>
                EffectResponse.Retry(redemptionId: "r-1", reason: null, retryAfterMs: 5000);

            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Duplicate_throws_when_redemption_id_is_null()
        {
            Action act = () => EffectResponse.Duplicate(redemptionId: null);

            act.Should().Throw<ArgumentNullException>();
        }
    }
}
