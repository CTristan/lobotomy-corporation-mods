// SPDX-License-Identifier: MIT

#region

using System;
using System.Collections.Generic;
using FluentAssertions;
using JetBrains.Annotations;
using LobotomyCorporationMods.BadLuckProtectionForGifts;
using LobotomyCorporationMods.BadLuckProtectionForGifts.Interfaces;
using LobotomyCorporationMods.BadLuckProtectionForGifts.Patches;
using Moq;
using Xunit;
using Xunit.Extensions;

#endregion

namespace LobotomyCorporationMods.Test.BadLuckProtectionForGiftsTests
{
    public sealed class BadLuckProtectionForGiftsTests
    {
        private const string GiftName = "DefaultGiftName";

        [Theory]
        [InlineData(0F)]
        [InlineData(0.1F)]
        [InlineData(1F)]
        public void A_gift_that_has_not_been_worked_on_yet_displays_the_base_value(float expected)
        {
            var instance = TestExtensions.CreateCreatureEquipmentMakeInfo();
            var actual = expected;

            CreatureEquipmentMakeInfoPatchGetProb.Postfix(instance, ref actual);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Restarting_the_day_reloads_the_saved_data_and_overwrites_the_progress_made_that_day()
        {
            var mockAgentWorkTracker = CreateMockAgentWorkTracker();

            GameSceneControllerPatchOnStageStart.Postfix();

            mockAgentWorkTracker.Verify(tracker => tracker.Load(), Times.Once);
        }

        [Fact]
        public void Starting_a_new_game_resets_the_tracker()
        {
            var mockAgentWorkTracker = CreateMockAgentWorkTracker();

            NewTitleScriptPatchOnClickNewGame.Postfix();

            mockAgentWorkTracker.Verify(tracker => tracker.Reset(), Times.Once);
        }


        [Theory]
        [InlineData(1f)]
        [InlineData(2f)]
        [InlineData(3f)]
        public void The_gift_probability_increases_by_one_percent_for_every_success_the_agent_has_while_working(float numberOfSuccesses)
        {
            // Arrange
            var mockAgentWorkTracker = CreateMockAgentWorkTracker();
            mockAgentWorkTracker.Setup(tracker => tracker.GetLastAgentWorkCountByGift(GiftName)).Returns(numberOfSuccesses);

            var creatureEquipmentMakeInfo = GetCreatureEquipmentMakeInfo(GiftName);
            var expected = numberOfSuccesses / 100f;

            // Act
            var actual = 0f;
            CreatureEquipmentMakeInfoPatchGetProb.Postfix(creatureEquipmentMakeInfo, ref actual);

            // Assert
            actual.Should().Be(expected);
        }

        [Fact]
        public void Our_probability_bonus_does_not_cause_the_gift_probability_to_go_over_100_percent()
        {
            // Arrange
            var mockAgentWorkTracker = CreateMockAgentWorkTracker();
            var creatureEquipmentMakeInfo = GetCreatureEquipmentMakeInfo(GiftName);

            // 101 times worked would equal 101% bonus normally
            mockAgentWorkTracker.Setup(tracker => tracker.GetLastAgentWorkCountByGift(GiftName)).Returns(101);

            // Act
            var actual = 0f;
            CreatureEquipmentMakeInfoPatchGetProb.Postfix(creatureEquipmentMakeInfo, ref actual);

            // Assert
            // We should only get back 100% even with the 101% bonus
            const float Expected = 1f;
            actual.Should().Be(Expected);
        }

        [Fact]
        public void The_tracker_data_is_saved_when_going_to_the_next_day()
        {
            var mockAgentWorkTracker = CreateMockAgentWorkTracker();

            GameSceneControllerPatchOnClickNextDay.Postfix();

            mockAgentWorkTracker.Verify(tracker => tracker.Save(), Times.Once);
        }


        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(10)]
        public void Working_on_an_abnormality_increases_the_number_of_successes_for_that_agent(int numberOfSuccesses)
        {
            var mockAgentWorkTracker = CreateMockAgentWorkTracker();
            var useSkill = TestExtensions.CreateUseSkill();
            var creatureEquipmentMakeInfo = GetCreatureEquipmentMakeInfo(GiftName);
            useSkill.targetCreature.metaInfo.equipMakeInfos.Add(creatureEquipmentMakeInfo);
            useSkill.successCount = numberOfSuccesses;

            UseSkillPatchFinishWorkSuccessfully.Prefix(useSkill);

            mockAgentWorkTracker.Verify(tracker => tracker.IncrementAgentWorkCount(GiftName, It.IsAny<long>(), numberOfSuccesses), Times.Once);
        }

        #region Harmony Tests

        /// <summary>
        ///     Harmony requires the constructor to be public.
        /// </summary>
        [Fact]
        public void Constructor_is_public_and_externally_accessible()
        {
            Action act = () => _ = new Harmony_Patch();
            act.ShouldNotThrow();
        }

        #endregion

        #region Helper Methods

        [NotNull]
        private static Mock<IAgentWorkTracker> CreateMockAgentWorkTracker()
        {
            var mockAgentTracker = new Mock<IAgentWorkTracker>();
            Harmony_Patch.Instance.LoadTracker(mockAgentTracker.Object);

            return mockAgentTracker;
        }

        [NotNull]
        private static CreatureEquipmentMakeInfo GetCreatureEquipmentMakeInfo([NotNull] string giftName)
        {
            var equipTypeInfo = TestExtensions.CreateEquipmentTypeInfo();
            equipTypeInfo.type = EquipmentTypeInfo.EquipmentType.SPECIAL;
            equipTypeInfo.localizeData = new Dictionary<string, string> { { "name", giftName } };

            var creatureEquipmentMakeInfo = TestExtensions.CreateCreatureEquipmentMakeInfo();
            creatureEquipmentMakeInfo.equipTypeInfo = equipTypeInfo;

            LocalizeTextDataModel.instance.Init(new Dictionary<string, string> { { giftName, giftName } });

            return creatureEquipmentMakeInfo;
        }

        #endregion
    }
}
