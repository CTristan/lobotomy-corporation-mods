// SPDX-License-Identifier: MIT

#region

using FluentAssertions;
using LobotomyCorporationMods.BadLuckProtectionForGifts;
using LobotomyCorporationMods.BadLuckProtectionForGifts.Interfaces;
using LobotomyCorporationMods.BadLuckProtectionForGifts.Patches;
using LobotomyCorporationMods.Test.Extensions;
using Moq;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Test.ModTests.BadLuckProtectionForGiftsTests.PatchTests
{
    public sealed class CreatureEquipmentMakeInfoPatchGetProbTests
        : BadLuckProtectionForGiftsModTests
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
            var mockConfig = CreateMockConfig();

            // Act
            var actual = sut.PatchAfterGetProb(
                expected,
                mockAgentWorkTracker.Object,
                mockConfig.Object
            );

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
            var mockConfig = CreateMockConfig();

            // Act
            var actual = sut.PatchAfterGetProb(0f, mockAgentWorkTracker.Object, mockConfig.Object);

            // Assert
            actual.Should().Be(Expected);
        }

        [Fact]
        public void Our_probability_bonus_does_not_cause_the_gift_probability_to_go_over_100_percent()
        {
            // Arrange
            var sut = GetCreatureEquipmentMakeInfo(GiftName);

            // 101 times worked would equal 101% bonus normally
            const int TimesWorked = 101;

            var mockAgentWorkTracker = new Mock<IAgentWorkTracker>();
            var mockConfig = CreateMockConfig();

            mockAgentWorkTracker
                .Setup(tracker => tracker.GetLastAgentWorkCountByGift(GiftName))
                .Returns(TimesWorked);

            // Act
            var actual = sut.PatchAfterGetProb(0f, mockAgentWorkTracker.Object, mockConfig.Object);

            // Assert
            // We should only get back 100% even with the 101% bonus
            const float Expected = 1f;
            actual.Should().Be(Expected);
        }

        [Theory]
        [InlineData(1f)]
        [InlineData(2f)]
        [InlineData(3f)]
        public void The_gift_probability_increases_by_one_percent_for_every_success_the_agent_has_while_working(
            float numberOfSuccesses
        )
        {
            // Arrange
            var sut = GetCreatureEquipmentMakeInfo(GiftName);
            var expected = numberOfSuccesses / 100f;

            var mockAgentWorkTracker = new Mock<IAgentWorkTracker>();
            var mockConfig = CreateMockConfig();
            mockAgentWorkTracker
                .Setup(tracker => tracker.GetLastAgentWorkCountByGift(GiftName))
                .Returns(numberOfSuccesses);

            // Act
            var actual = sut.PatchAfterGetProb(0f, mockAgentWorkTracker.Object, mockConfig.Object);

            // Assert
            actual.Should().Be(expected);
        }

        [Theory]
        [InlineData(RiskLevel.ZAYIN, 2.0f)]
        [InlineData(RiskLevel.ALEPH, 0.5f)]
        [InlineData(RiskLevel.WAW, 3.0f)]
        public void Different_risk_levels_use_different_bonus_percentages(
            RiskLevel riskLevel,
            float bonusPercentage
        )
        {
            // Arrange
            var sut = GetCreatureEquipmentMakeInfo(GiftName);
            const float WorkCount = 10f;
            var expected = WorkCount * bonusPercentage / 100f;

            var mockAgentWorkTracker = new Mock<IAgentWorkTracker>();
            mockAgentWorkTracker
                .Setup(tracker => tracker.GetLastAgentWorkCountByGift(GiftName))
                .Returns(WorkCount);
            mockAgentWorkTracker
                .Setup(tracker => tracker.GetRiskLevelByGift(GiftName))
                .Returns(riskLevel);

            var mockConfig = new Mock<IBadLuckProtectionConfig>();
            mockConfig
                .Setup(c => c.GetBonusPercentageForRiskLevel(riskLevel))
                .Returns(bonusPercentage);

            // Act
            var actual = sut.PatchAfterGetProb(0f, mockAgentWorkTracker.Object, mockConfig.Object);

            // Assert
            actual.Should().Be(expected);
        }

        [Fact]
        public void Unknown_risk_level_defaults_to_one_percent()
        {
            // Arrange
            var sut = GetCreatureEquipmentMakeInfo(GiftName);
            const float WorkCount = 10f;
            const float Expected = WorkCount / 100f;

            var mockAgentWorkTracker = new Mock<IAgentWorkTracker>();
            mockAgentWorkTracker
                .Setup(tracker => tracker.GetLastAgentWorkCountByGift(GiftName))
                .Returns(WorkCount);
            mockAgentWorkTracker
                .Setup(tracker => tracker.GetRiskLevelByGift(GiftName))
                .Returns((RiskLevel?)null);

            var mockConfig = CreateMockConfig();

            // Act
            var actual = sut.PatchAfterGetProb(0f, mockAgentWorkTracker.Object, mockConfig.Object);

            // Assert
            actual.Should().Be(Expected);
        }

        [Fact]
        public void Zero_bonus_percentage_produces_no_bonus()
        {
            // Arrange
            var sut = GetCreatureEquipmentMakeInfo(GiftName);
            const float WorkCount = 50f;

            var mockAgentWorkTracker = new Mock<IAgentWorkTracker>();
            mockAgentWorkTracker
                .Setup(tracker => tracker.GetLastAgentWorkCountByGift(GiftName))
                .Returns(WorkCount);
            mockAgentWorkTracker
                .Setup(tracker => tracker.GetRiskLevelByGift(GiftName))
                .Returns(RiskLevel.ZAYIN);

            var mockConfig = new Mock<IBadLuckProtectionConfig>();
            mockConfig.Setup(c => c.GetBonusPercentageForRiskLevel(RiskLevel.ZAYIN)).Returns(0f);

            // Act
            var actual = sut.PatchAfterGetProb(0f, mockAgentWorkTracker.Object, mockConfig.Object);

            // Assert
            actual.Should().Be(0f);
        }

        [Fact]
        public void Normalized_mode_applies_risk_level_percentage_to_normalized_work_count()
        {
            // Arrange - normalized work count of 3.5 with 2x ALEPH multiplier
            var sut = GetCreatureEquipmentMakeInfo(GiftName);
            const float WorkCount = 3.5f;
            const float AlephMultiplier = 2.0f;
            const float Expected = WorkCount * AlephMultiplier / 100f;

            var mockAgentWorkTracker = new Mock<IAgentWorkTracker>();
            mockAgentWorkTracker
                .Setup(tracker => tracker.GetLastAgentWorkCountByGift(GiftName))
                .Returns(WorkCount);
            mockAgentWorkTracker
                .Setup(tracker => tracker.GetRiskLevelByGift(GiftName))
                .Returns(RiskLevel.ALEPH);

            var mockConfig = new Mock<IBadLuckProtectionConfig>();
            mockConfig.Setup(c => c.BonusCalculationMode).Returns(BonusCalculationMode.Normalized);
            mockConfig
                .Setup(c => c.GetBonusPercentageForRiskLevel(RiskLevel.ALEPH))
                .Returns(AlephMultiplier);

            // Act
            var actual = sut.PatchAfterGetProb(0f, mockAgentWorkTracker.Object, mockConfig.Object);

            // Assert - 3.5 * 2.0 / 100 = 0.07
            actual.Should().Be(Expected);
        }

        private static Mock<IBadLuckProtectionConfig> CreateMockConfig(
            BonusCalculationMode bonusCalculationMode = BonusCalculationMode.PerPEBox
        )
        {
            var mock = new Mock<IBadLuckProtectionConfig>();
            mock.Setup(c => c.ResetOnGiftReceived).Returns(false);
            mock.Setup(c => c.BonusCalculationMode).Returns(bonusCalculationMode);
            mock.Setup(c => c.GetBonusPercentageForRiskLevel(It.IsAny<RiskLevel>())).Returns(1.0f);

            return mock;
        }
    }
}
