// SPDX-License-Identifier: MIT

using System;
using System.Globalization;
using System.IO;
using FluentAssertions;
using JetBrains.Annotations;
using LobotomyCorporationMods.BadLuckProtectionForGifts;
using LobotomyCorporationMods.BadLuckProtectionForGifts.Interfaces;
using LobotomyCorporationMods.BadLuckProtectionForGifts.Patches;
using NSubstitute;
using Xunit;
using Xunit.Extensions;

namespace LobotomyCorporationMods.Test
{
    public sealed class BadLuckProtectionForGiftsTests
    {
        private const long AgentId = 1;
        private const string GiftName = "Test";

        /// <summary>
        ///     Harmony requires the constructor to be public.
        /// </summary>
        [Fact]
        public void Constructor_is_public_and_externally_accessible()
        {
            Action act = () => _ = new Harmony_Patch();
            act.ShouldNotThrow();
        }

        [Fact]
        public void Converting_a_tracker_to_a_string_with_multiple_gifts_and_agents_contains_all_of_the_data_in_the_tracker()
        {
            const string DataFileName = "Converting_a_tracker_to_a_string_with_multiple_gifts_and_agents_contains_all_of_the_data_in_the_tracker";
            const string SecondGiftName = "Second";
            const long SecondAgentId = AgentId + 1;
            var agentWorkTracker = CreateAgentWorkTracker(DataFileName);

            // First gift first agent
            agentWorkTracker.IncrementAgentWorkCount(GiftName, AgentId);

            // First gift second agent
            agentWorkTracker.IncrementAgentWorkCount(GiftName, SecondAgentId);

            // Second gift second agent
            agentWorkTracker.IncrementAgentWorkCount(SecondGiftName, SecondAgentId, 2f);
            var expected = string.Format(CultureInfo.CurrentCulture, "{0}^{1};1^{2};1|{3}^{2};2", GiftName, AgentId.ToString(CultureInfo.CurrentCulture),
                SecondAgentId.ToString(CultureInfo.CurrentCulture), SecondGiftName);

            var actual = agentWorkTracker.ToString();

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Converting_a_tracker_to_a_string_with_a_single_gift_and_a_single_agent_returns_the_correct_string()
        {
            const string DataFileName = "Converting_a_tracker_to_a_string_with_a_single_gift_and_a_single_agent_returns_the_correct_string";
            var agentWorkTracker = CreateAgentWorkTracker(DataFileName);
            agentWorkTracker.IncrementAgentWorkCount(GiftName, AgentId);
            var expected = $@"{GiftName}^{AgentId.ToString(CultureInfo.CurrentCulture)};1";

            var actual = agentWorkTracker.ToString();

            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("Test^1;1", GiftName, AgentId, 1f)]
        [InlineData("Test^1;1^2;2", GiftName, 2, 2f)]
        [InlineData("Test^1;1^2;2|Second^1;3", "Second", 1, 3f)]
        public void Loading_data_from_a_saved_tracker_file_populates_a_valid_tracker([NotNull] string trackerData, [NotNull] string giftName, long agentId, float numberOfTimes)
        {
            var dataFileName = $"Loading_data_with_a_saved_tracker_file_populates_a_valid_tracker_{giftName}_{agentId}_{numberOfTimes}";
            var agentWorkTracker = CreateAgentWorkTracker(dataFileName, trackerData);

            Assert.Equal(numberOfTimes, agentWorkTracker.GetLastAgentWorkCountByGift(giftName));
        }

        [Theory]
        [InlineData("Test^1;1", GiftName, AgentId, 1f)]
        [InlineData("Test^1;1^2;2", GiftName, 2, 2f)]
        [InlineData("Test^1;1^2;2|Second^1;3", "Second", 1, 3f)]
        public void Loading_data_multiple_times_from_a_saved_tracker_file_does_not_duplicate_work_progress([NotNull] string trackerData, [NotNull] string giftName, long agentId, float numberOfTimes)
        {
            var dataFileName = $"Loading_data_with_a_saved_tracker_file_populates_a_valid_tracker_{giftName}_{agentId}_{numberOfTimes}";
            var agentWorkTracker = CreateAgentWorkTracker(dataFileName, trackerData);

            agentWorkTracker.Load();
            agentWorkTracker.Load();

            Assert.Equal(numberOfTimes, agentWorkTracker.GetLastAgentWorkCountByGift(giftName));
        }

        [Fact]
        public void Our_probability_bonus_does_not_cause_the_gift_probability_to_go_over_100_percent()
        {
            const string DataFileName = "Our_probability_bonus_does_not_cause_the_gift_probability_to_go_over_100_percent";
            var agentWorkTracker = CreateAgentWorkTracker(DataFileName);
            var creatureEquipmentMakeInfo = TestExtensions.CreateCreatureEquipmentMakeInfo(GiftName);

            // 101 times worked would equal 101% bonus normally
            agentWorkTracker.IncrementAgentWorkCount(GiftName, AgentId, 101f);

            // We should only get back 100% even with the 101% bonus
            const float Expected = 1f;

            var actual = 0f;
            CreatureEquipmentMakeInfoPatchGetProb.Postfix(creatureEquipmentMakeInfo, ref actual);

            Assert.Equal(Expected, actual);
        }

        [Theory]
        [InlineData(1f)]
        [InlineData(2f)]
        [InlineData(3f)]
        public void The_gift_probability_increases_by_one_percent_for_every_success_the_agent_has_while_working(float numberOfSuccesses)
        {
            var dataFileName = $"The_gift_probability_increases_by_one_percent_for_every_success_the_agent_has_while_working_{numberOfSuccesses}";
            var agentWorkTracker = CreateAgentWorkTracker(dataFileName);
            agentWorkTracker.IncrementAgentWorkCount(GiftName, AgentId, numberOfSuccesses);
            var creatureEquipmentMakeInfo = TestExtensions.CreateCreatureEquipmentMakeInfo(GiftName);
            var expected = numberOfSuccesses / 100f;

            var actual = 0f;
            CreatureEquipmentMakeInfoPatchGetProb.Postfix(creatureEquipmentMakeInfo, ref actual);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Starting_a_new_game_resets_the_tracker()
        {
            const int ExpectedWorkCount = 0;
            const string DataFileName = "Starting_a_new_game_reloads_the_last_saved_tracker_progress";
            var agentWorkTracker = CreateAgentWorkTracker(DataFileName);
            agentWorkTracker.IncrementAgentWorkCount(GiftName, AgentId);

            NewTitleScriptPatchOnClickNewGame.Postfix();
            agentWorkTracker = Harmony_Patch.Instance.AgentWorkTracker;
            var actualWorkCount = agentWorkTracker.GetLastAgentWorkCountByGift(GiftName);

            Assert.Equal(ExpectedWorkCount, actualWorkCount);
        }


        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(10)]
        public void Working_on_an_abnormality_increases_the_number_of_successes_for_that_agent(int numberOfSuccesses)
        {
            var dataFileName = $"Working_on_an_abnormality_increases_the_number_of_successes_for_that_agent_{numberOfSuccesses}";
            var agentWorkTracker = CreateAgentWorkTracker(dataFileName);
            var useSkill = TestExtensions.CreateUseSkill(GiftName, AgentId, numberOfSuccesses);
            agentWorkTracker.IncrementAgentWorkCount(GiftName, AgentId);
            var expected = agentWorkTracker.GetLastAgentWorkCountByGift(GiftName) + numberOfSuccesses;

            UseSkillPatchFinishWorkSuccessfully.Prefix(useSkill);
            var actual = agentWorkTracker.GetLastAgentWorkCountByGift(GiftName);

            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(0F)]
        [InlineData(0.1F)]
        [InlineData(1F)]
        public void A_gift_that_has_not_been_worked_on_yet_displays_the_base_value(float expected)
        {
            var dataFileName = $"A_gift_that_has_not_been_worked_on_yet_displays_the_base_value_{expected}";
            _ = CreateAgentWorkTracker(dataFileName);
            var instance = TestExtensions.CreateCreatureEquipmentMakeInfo(GiftName);
            var actual = expected;

            CreatureEquipmentMakeInfoPatchGetProb.Postfix(instance, ref actual);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void The_tracker_data_is_saved_when_going_to_the_next_day()
        {
            const string DataFileName = "The_tracker_data_is_saved_when_going_to_the_next_day";
            var agentWorkTracker = CreateAgentWorkTracker(DataFileName);

            GameSceneControllerPatchOnClickNextDay.Postfix();

            AssertSaveTrackerCalled(agentWorkTracker);
        }

        [Theory]
        [InlineData("Test^1;1", GiftName, AgentId, 1f)]
        [InlineData("Test^1;1^2;2", GiftName, 2, 2f)]
        [InlineData("Test^1;1^2;2|Second^1;3", "Second", 1, 3f)]
        public void Restarting_the_day_reloads_the_saved_data_and_overwrites_the_progress_made_that_day([NotNull] string trackerData, [NotNull] string giftName, long agentId, float numberOfTimes)
        {
            var dataFileName = $"Restarting_the_day_reloads_the_saved_data_and_overwrites_the_progress_made_that_day_{giftName}_{agentId}_{numberOfTimes}";
            var agentWorkTracker = CreateAgentWorkTracker(dataFileName, trackerData);

            agentWorkTracker.IncrementAgentWorkCount(giftName, agentId);
            GameSceneControllerPatchOnStageStart.Postfix();

            Assert.Equal(numberOfTimes, agentWorkTracker.GetLastAgentWorkCountByGift(giftName));
        }

        #region Test Helper Methods

        private static void AssertSaveTrackerCalled([NotNull] IAgentWorkTracker agentWorkTracker)
        {
            Harmony_Patch.Instance.FileManager.Received().WriteAllText(Arg.Any<string>(), agentWorkTracker.ToString());
        }

        /// <summary>
        ///     Populates the Harmony Patch with an agent work tracker pointed to our specified test data file.
        /// </summary>
        [NotNull]
        private IAgentWorkTracker CreateAgentWorkTracker(string dataFileName, string trackerData = "")
        {
            var fileManager = TestExtensions.CreateFileManager();
            dataFileName = dataFileName.InCurrentDirectory();
            CreateTestTrackerFile(dataFileName, trackerData);
            Harmony_Patch.Instance.LoadData(fileManager, dataFileName);

            return Harmony_Patch.Instance.AgentWorkTracker;
        }

        private static void CreateTestTrackerFile([NotNull] string fileName, string trackerData)
        {
            var fileNameWithPath = Path.Combine(Directory.GetCurrentDirectory(), fileName);
            File.WriteAllText(fileNameWithPath, trackerData);
        }

        #endregion
    }
}
