// SPDX-License-Identifier: MIT

#region

using FluentAssertions;
using LobotomyCorporationMods.BadLuckProtectionForGifts.Interfaces;
using LobotomyCorporationMods.BadLuckProtectionForGifts.Patches;
using LobotomyCorporationMods.Test.Extensions;
using Moq;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Test.Mods.BadLuckProtectionForGifts.Patches
{
    public sealed class CreatureEquipmentMakeInfoPatchGetProbTests : BadLuckProtectionForGiftsTests
    {
        [Theory]
        [InlineData(0F)]
        [InlineData(0.1F)]
        [InlineData(1F)]
        public void A_gift_that_has_not_been_worked_on_yet_displays_the_base_value(float expected)
        {
            // Arrange
            var sut = UnityTestExtensions.CreateCreatureEquipmentMakeInfo();

            var mockAgentWorkTracker = new Mock<IAgentWorkTracker>();

            // Act
            var actual = sut.PatchAfterGetProb(expected, mockAgentWorkTracker.Object);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void An_abnormality_with_no_gift_does_not_increase_probability_chance()
        {
            // Arrange
            var sut = UnityTestExtensions.CreateCreatureEquipmentMakeInfo();
            const float Expected = 0f;

            sut.equipTypeInfo = null;
            var mockAgentWorkTracker = new Mock<IAgentWorkTracker>();

            // Act
            var actual = 0f;
            actual = sut.PatchAfterGetProb(actual, mockAgentWorkTracker.Object);

            // Assert
            actual.Should().Be(Expected);
        }

        [Fact]
        public void Our_probability_bonus_does_not_cause_the_gift_probability_to_go_over_100_percent()
        {
            // Arrange
            var sut = GetCreatureEquipmentMakeInfo(GiftName);

            var mockAgentWorkTracker = new Mock<IAgentWorkTracker>();

            // 101 times worked would equal 101% bonus normally
            mockAgentWorkTracker.Setup(tracker => tracker.GetLastAgentWorkCountByGift(GiftName)).Returns(101);

            // Act
            var actual = 0f;
            actual = sut.PatchAfterGetProb(actual, mockAgentWorkTracker.Object);

            // Assert
            // We should only get back 100% even with the 101% bonus
            const float Expected = 1f;
            actual.Should().Be(Expected);
        }

        [Theory]
        [InlineData(1f)]
        [InlineData(2f)]
        [InlineData(3f)]
        public void The_gift_probability_increases_by_one_percent_for_every_success_the_agent_has_while_working(float numberOfSuccesses)
        {
            // Arrange
            var sut = GetCreatureEquipmentMakeInfo(GiftName);
            var expected = numberOfSuccesses / 100f;

            var mockAgentWorkTracker = new Mock<IAgentWorkTracker>();
            mockAgentWorkTracker.Setup(tracker => tracker.GetLastAgentWorkCountByGift(GiftName)).Returns(numberOfSuccesses);

            // Act
            var actual = 0f;
            actual = sut.PatchAfterGetProb(actual, mockAgentWorkTracker.Object);

            // Assert
            actual.Should().Be(expected);
        }
    }
}
