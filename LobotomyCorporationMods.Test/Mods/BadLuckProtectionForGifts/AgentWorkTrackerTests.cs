// SPDX-License-Identifier: MIT

#region

using System.Collections.Generic;
using System.Globalization;
using System.IO;
using FluentAssertions;
using JetBrains.Annotations;
using LobotomyCorporationMods.BadLuckProtectionForGifts.Implementations;
using LobotomyCorporationMods.Common.Interfaces;
using LobotomyCorporationMods.Test.Extensions;
using Moq;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Test.Mods.BadLuckProtectionForGifts
{
    public sealed class AgentWorkTrackerTests
    {
        private const string DefaultGiftName = "DefaultGiftName";

        /// <summary>
        /// Provides test data for the <see cref="AgentWorkTrackerTests"/> class.
        /// </summary>
        [NotNull]
        public static IEnumerable<object[]> TrackerTestData
        {
            get
            {
                const string FirstGiftData = DefaultGiftName + "^1;1";
                const string SecondGiftData = DefaultGiftName + "^1;1^2;2";
                const string ThirdGiftData = DefaultGiftName + "^1;1^2;2|Second^1;3";
                const string SecondGiftNameSet = "Second";
                const long FirstAgentId = 1L;
                const float FirstAgentWorkCount = 1f;
                const long SecondAgentId = 2L;
                const float SecondAgentWorkCount = 2f;
                const float ThirdAgentWorkCount = 3f;

                var firstAgentFirstGift = new object[]
                {
                    FirstGiftData, DefaultGiftName, FirstAgentId, FirstAgentWorkCount,
                };
                var secondAgentFirstGift = new object[]
                {
                    SecondGiftData, DefaultGiftName, SecondAgentId, SecondAgentWorkCount,
                };
                var firstAgentSecondGift = new object[]
                {
                    ThirdGiftData, SecondGiftNameSet, FirstAgentId, ThirdAgentWorkCount,
                };

                return new List<object[]>
                {
                    firstAgentFirstGift,
                    secondAgentFirstGift,
                    firstAgentSecondGift,
                };
            }
        }

        [Fact]
        public void Converting_a_tracker_to_a_string_with_a_single_gift_and_a_single_agent_returns_the_correct_string()
        {
            const string DataFileName = nameof(Converting_a_tracker_to_a_string_with_a_single_gift_and_a_single_agent_returns_the_correct_string);
            var agentWorkTracker = CreateAgentWorkTracker(DataFileName);
            agentWorkTracker.IncrementAgentWorkCount(DefaultGiftName, 1L);
            var expected = $"{DefaultGiftName}^{1L.ToString(CultureInfo.CurrentCulture)};1";

            var actual = agentWorkTracker.ToString();

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Converting_a_tracker_to_a_string_with_multiple_gifts_and_agents_contains_all_of_the_data_in_the_tracker()
        {
            // Arrange
            const string DataFileName = nameof(Converting_a_tracker_to_a_string_with_multiple_gifts_and_agents_contains_all_of_the_data_in_the_tracker);
            const string SecondGiftName = "Second";
            const long SecondAgentId = 1L + 1;
            var agentWorkTracker = CreateAgentWorkTracker(DataFileName);

            // First gift first agent
            agentWorkTracker.IncrementAgentWorkCount(DefaultGiftName, 1L);

            // First gift second agent
            agentWorkTracker.IncrementAgentWorkCount(DefaultGiftName, SecondAgentId);

            // Second gift second agent
            agentWorkTracker.IncrementAgentWorkCount(SecondGiftName, SecondAgentId, 2f);
            var expected = string.Format(CultureInfo.CurrentCulture, "{0}^{1};1^{2};1|{3}^{2};2", DefaultGiftName, 1L.ToString(CultureInfo.CurrentCulture),
                SecondAgentId.ToString(CultureInfo.CurrentCulture), SecondGiftName);

            // Act
            var actual = agentWorkTracker.ToString();

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Getting_last_agent_work_count_when_gift_not_yet_worked_on_returns_zero()
        {
            const string DataFileName = nameof(Getting_last_agent_work_count_when_gift_not_yet_worked_on_returns_zero);
            var agentWorkTracker = CreateAgentWorkTracker(DataFileName);

            var result = agentWorkTracker.GetLastAgentWorkCountByGift(DefaultGiftName);

            result.Should().Be(0f);
        }

        [Theory]
        [MemberData(nameof(TrackerTestData))]
        public void Loading_data_from_a_saved_tracker_file_populates_a_valid_tracker(string trackerData,
            [NotNull] string giftName,
            long agentId,
            float numberOfTimes)
        {
            // Arrange
            var dataFileName = $"{nameof(Loading_data_from_a_saved_tracker_file_populates_a_valid_tracker)}_{giftName}_{agentId}_{numberOfTimes}";
            var agentWorkTracker = CreateAgentWorkTracker(dataFileName, trackerData);

            // Act
            var result = agentWorkTracker.GetLastAgentWorkCountByGift(giftName);

            // Assert
            result.Should().Be(numberOfTimes);
        }

        [Theory]
        [MemberData(nameof(TrackerTestData))]
        public void Loading_data_multiple_times_from_a_saved_tracker_file_does_not_duplicate_work_progress(string trackerData,
            [NotNull] string giftName,
            long agentId,
            float numberOfTimes)
        {
            var appendText = $"_{giftName}_{agentId}_{numberOfTimes}";
            var dataFileName = nameof(Loading_data_multiple_times_from_a_saved_tracker_file_does_not_duplicate_work_progress).ShortenBy(appendText.Length);
            dataFileName += appendText;
            var agentWorkTracker = CreateAgentWorkTracker(dataFileName, trackerData);

            agentWorkTracker.Load();
            agentWorkTracker.Load();

            Assert.Equal(numberOfTimes, agentWorkTracker.GetLastAgentWorkCountByGift(giftName));
        }

        [Fact]
        public void Resetting_tracker_clears_all_gifts_and_saves_the_file()
        {
            // Arrange
            const string DataFileName = nameof(Resetting_tracker_clears_all_gifts_and_saves_the_file);
            const string TrackerData = DefaultGiftName + "^1;1";

            var mockFileManager = TestExtensions.GetMockFileManager();
            var agentWorkTracker = CreateAgentWorkTracker(DataFileName, TrackerData, mockFileManager.Object);
            agentWorkTracker.Load();

            // Quick sanity check that we actually have gifts loaded
            agentWorkTracker.GetLastAgentWorkCountByGift(DefaultGiftName).Should().BeGreaterThan(0);

            // Act
            agentWorkTracker.Reset();

            // Assert
            agentWorkTracker.GetLastAgentWorkCountByGift(DefaultGiftName).Should().Be(0);
            mockFileManager.Verify(manager => manager.WriteAllText(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        #region Helper Methods

        /// <summary>Populates the Harmony Patch with an agent work tracker pointed to our specified test data file.</summary>
        [NotNull]
        private static AgentWorkTracker CreateAgentWorkTracker(string dataFileName,
            string trackerData = "",
            IFileManager fileManager = null)
        {
            fileManager = fileManager ?? TestExtensions.GetMockFileManager().Object;
            dataFileName = dataFileName.InCurrentDirectory();
            CreateTestTrackerFile(dataFileName, trackerData);

            return new AgentWorkTracker(fileManager, dataFileName);
        }

        private static void CreateTestTrackerFile([NotNull] string fileName,
            string trackerData)
        {
            var fileNameWithPath = Path.Combine(Directory.GetCurrentDirectory(), fileName);
            File.WriteAllText(fileNameWithPath, trackerData);
        }

        #endregion
    }
}
