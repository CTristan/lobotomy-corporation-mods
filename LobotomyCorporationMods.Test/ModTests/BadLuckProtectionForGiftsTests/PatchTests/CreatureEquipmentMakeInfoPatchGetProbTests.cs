// SPDX-License-Identifier: MIT

#region

using AwesomeAssertions;
using LobotomyCorporationMods.BadLuckProtectionForGifts.Interfaces;
using LobotomyCorporationMods.BadLuckProtectionForGifts.Patches;
using LobotomyCorporationMods.Test.Extensions;
using Moq;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Test.ModTests.BadLuckProtectionForGiftsTests.PatchTests
{
    public sealed class CreatureEquipmentMakeInfoPatchGetProbTests : BadLuckProtectionForGiftsModTests
    {
        [Theory]
        [InlineData(0F)]
        [InlineData(0.1F)]
        [InlineData(1F)]
        public void A_gift_that_has_not_been_worked_on_yet_displays_the_base_value(float expected)
        {
            // Arrange
            CreatureEquipmentMakeInfo sut = UnityTestExtensions.CreateCreatureEquipmentMakeInfo();

            Mock<IAgentWorkTracker> mockAgentWorkTracker = new();

            // Act
            float actual = sut.PatchAfterGetProb(expected, mockAgentWorkTracker.Object);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void An_abnormality_with_no_gift_does_not_increase_probability_chance()
        {
            // Arrange
            CreatureEquipmentMakeInfo sut = UnityTestExtensions.CreateCreatureEquipmentMakeInfo();
            const float Expected = 0f;

            sut.equipTypeInfo = null;
            Mock<IAgentWorkTracker> mockAgentWorkTracker = new();

            // Act
            float actual = 0f;
            actual = sut.PatchAfterGetProb(actual, mockAgentWorkTracker.Object);

            // Assert
            _ = actual.Should().Be(Expected);
        }

        [Fact]
        public void Our_probability_bonus_does_not_cause_the_gift_probability_to_go_over_100_percent()
        {
            // Arrange
            CreatureEquipmentMakeInfo sut = GetCreatureEquipmentMakeInfo(GiftName);

            // 101 times worked would equal 101% bonus normally
            const int TimesWorked = 101;

            Mock<IAgentWorkTracker> mockAgentWorkTracker = new();

            _ = mockAgentWorkTracker.Setup(tracker => tracker.GetLastAgentWorkCountByGift(GiftName)).Returns(TimesWorked);

            // Act
            float actual = 0f;
            actual = sut.PatchAfterGetProb(actual, mockAgentWorkTracker.Object);

            // Assert
            // We should only get back 100% even with the 101% bonus
            const float Expected = 1f;
            _ = actual.Should().Be(Expected);
        }

        [Theory]
        [InlineData(1f)]
        [InlineData(2f)]
        [InlineData(3f)]
        public void The_gift_probability_increases_by_one_percent_for_every_success_the_agent_has_while_working(float numberOfSuccesses)
        {
            // Arrange
            CreatureEquipmentMakeInfo sut = GetCreatureEquipmentMakeInfo(GiftName);
            float expected = numberOfSuccesses / 100f;

            Mock<IAgentWorkTracker> mockAgentWorkTracker = new();
            _ = mockAgentWorkTracker.Setup(tracker => tracker.GetLastAgentWorkCountByGift(GiftName)).Returns(numberOfSuccesses);

            // Act
            float actual = 0f;
            actual = sut.PatchAfterGetProb(actual, mockAgentWorkTracker.Object);

            // Assert
            _ = actual.Should().Be(expected);
        }
    }
}
