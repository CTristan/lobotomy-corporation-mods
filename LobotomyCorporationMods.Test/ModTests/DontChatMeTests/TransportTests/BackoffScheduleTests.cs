// SPDX-License-Identifier: MIT

#region

using AwesomeAssertions;
using LobotomyCorporationMods.DontChatMe.Transport;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Test.ModTests.DontChatMeTests.TransportTests
{
    public sealed class BackoffScheduleTests : DontChatMeModTests
    {
        [Fact]
        public void First_attempt_uses_the_initial_delay()
        {
            BackoffSchedule
                .DelayFor(attempt: 1, randomFraction: 0)
                .Should()
                .Be(BackoffSchedule.InitialDelaySeconds);
        }

        [Fact]
        public void Each_subsequent_attempt_doubles_the_base_delay()
        {
            BackoffSchedule.DelayFor(2, 0).Should().Be(2f);
            BackoffSchedule.DelayFor(3, 0).Should().Be(4f);
            BackoffSchedule.DelayFor(4, 0).Should().Be(8f);
        }

        [Fact]
        public void Delay_is_capped_at_max_delay_seconds()
        {
            BackoffSchedule
                .DelayFor(attempt: 12, randomFraction: 0)
                .Should()
                .Be(BackoffSchedule.MaxDelaySeconds);
        }

        [Fact]
        public void Jitter_only_extends_the_delay_never_shortens_it()
        {
            BackoffSchedule
                .DelayFor(attempt: 1, randomFraction: 0.5)
                .Should()
                .BeGreaterThan(BackoffSchedule.InitialDelaySeconds);
            BackoffSchedule
                .DelayFor(attempt: 1, randomFraction: 0.5)
                .Should()
                .BeLessThanOrEqualTo(
                    BackoffSchedule.InitialDelaySeconds * (1f + BackoffSchedule.JitterFraction)
                );
        }

        [Fact]
        public void Zero_or_negative_attempt_is_clamped_to_attempt_one()
        {
            BackoffSchedule
                .DelayFor(attempt: 0, randomFraction: 0)
                .Should()
                .Be(BackoffSchedule.InitialDelaySeconds);
            BackoffSchedule
                .DelayFor(attempt: -5, randomFraction: 0)
                .Should()
                .Be(BackoffSchedule.InitialDelaySeconds);
        }
    }
}
