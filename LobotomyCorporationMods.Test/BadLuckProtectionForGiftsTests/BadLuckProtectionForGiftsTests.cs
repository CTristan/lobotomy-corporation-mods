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
            var dataFileName = $"A_gift_that_has_not_been_worked_on_yet_displays_the_base_value_{expected}";
            var instance = TestExtensions.CreateCreatureEquipmentMakeInfo();
            var actual = expected;

            CreatureEquipmentMakeInfoPatchGetProb.Postfix(instance, ref actual);

            Assert.Equal(expected, actual);
        }

        /// <summary>
        ///     Harmony requires the constructor to be public.
        /// </summary>
        [Fact]
        public void Constructor_is_public_and_externally_accessible()
        {
            Action act = () => _ = new Harmony_Patch();
            act.ShouldNotThrow();
        }

        [Theory]
        [InlineData(GiftName + "^1;1", GiftName, 1L, 1f)]
        [InlineData(GiftName + "^1;1^2;2", GiftName, 2L, 2f)]
        [InlineData("Test^1;1^2;2|Second^1;3", "Second", 1L, 3f)]
        public void Restarting_the_day_reloads_the_saved_data_and_overwrites_the_progress_made_that_day([NotNull] string trackerData, [NotNull] string giftName, long agentId, float numberOfTimes)
        {
            var dataFileName = $"Restarting_the_day_reloads_the_saved_data_and_overwrites_the_progress_made_that_day_{giftName}_{agentId}_{numberOfTimes}";
            var mockAgentWorkTracker = CreateMockAgentWorkTracker();

            GameSceneControllerPatchOnStageStart.Postfix();

            agentWorkTracker
        }

        [Fact]
        public void Starting_a_new_game_resets_the_tracker()
        {
            const int ExpectedWorkCount = 0;
            const string DataFileName = "Starting_a_new_game_reloads_the_last_saved_tracker_progress";
            var agentWorkTracker = CreateAgentWorkTracker(DataFileName);
            agentWorkTracker.IncrementAgentWorkCount(GiftName, 1L);

            NewTitleScriptPatchOnClickNewGame.Postfix();
            agentWorkTracker = Harmony_Patch.Instance.AgentWorkTracker;
            var actualWorkCount = agentWorkTracker.GetLastAgentWorkCountByGift(GiftName);

            Assert.Equal(ExpectedWorkCount, actualWorkCount);
        }


        [Theory]
        [InlineData(1f)]
        [InlineData(2f)]
        [InlineData(3f)]
        public void The_gift_probability_increases_by_one_percent_for_every_success_the_agent_has_while_working(float numberOfSuccesses)
        {
            var dataFileName = $"The_gift_probability_increases_by_one_percent_for_every_success_the_agent_has_while_working_{numberOfSuccesses}";
            var agentWorkTracker = CreateAgentWorkTracker(dataFileName);
            agentWorkTracker.IncrementAgentWorkCount(GiftName, 1L, numberOfSuccesses);
            var creatureEquipmentMakeInfo = GetCreatureEquipmentMakeInfo(GiftName);
            var expected = numberOfSuccesses / 100f;

            var actual = 0f;
            CreatureEquipmentMakeInfoPatchGetProb.Postfix(creatureEquipmentMakeInfo, ref actual);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void The_tracker_data_is_saved_when_going_to_the_next_day()
        {
            var mockFileManager = TestExtensions.GetMockFileManager();
            const string DataFileName = "The_tracker_data_is_saved_when_going_to_the_next_day";
            var agentWorkTracker = CreateAgentWorkTracker(DataFileName, fileManager: mockFileManager.Object);

            GameSceneControllerPatchOnClickNextDay.Postfix();

            mockFileManager.Verify(mock => mock.WriteAllText(It.IsAny<string>(), agentWorkTracker.ToString()), Times.Once);
        }


        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(10)]
        public void Working_on_an_abnormality_increases_the_number_of_successes_for_that_agent(int numberOfSuccesses)
        {
            var dataFileName = $"Working_on_an_abnormality_increases_the_number_of_successes_for_that_agent_{numberOfSuccesses}";
            var agentWorkTracker = CreateAgentWorkTracker(dataFileName);
            agentWorkTracker.IncrementAgentWorkCount(GiftName, 1L);
            var expected = agentWorkTracker.GetLastAgentWorkCountByGift(GiftName) + numberOfSuccesses;
            var useSkill = TestExtensions.CreateUseSkill();
            var creatureEquipmentMakeInfo = GetCreatureEquipmentMakeInfo(GiftName);
            useSkill.targetCreature.metaInfo.equipMakeInfos.Add(creatureEquipmentMakeInfo);
            useSkill.successCount = numberOfSuccesses;

            UseSkillPatchFinishWorkSuccessfully.Prefix(useSkill);
            var actual = agentWorkTracker.GetLastAgentWorkCountByGift(GiftName);

            Assert.Equal(expected, actual);
        }
        
        
        #region Helper Methods

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
