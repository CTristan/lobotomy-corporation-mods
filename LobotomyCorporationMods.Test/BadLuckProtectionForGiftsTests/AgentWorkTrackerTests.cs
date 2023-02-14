// SPDX-License-Identifier: MIT

#region

using System.Globalization;
using System.IO;
using FluentAssertions;
using JetBrains.Annotations;
using LobotomyCorporationMods.BadLuckProtectionForGifts.Implementations;
using LobotomyCorporationMods.BadLuckProtectionForGifts.Interfaces;
using LobotomyCorporationMods.Common.Interfaces;
using Xunit;
using Xunit.Extensions;

#endregion

namespace LobotomyCorporationMods.Test.BadLuckProtectionForGiftsTests
{
    public sealed class AgentWorkTrackerTests
    {
        private const string GiftName = "DefaultGiftName";

        [Fact]
        public void Converting_a_tracker_to_a_string_with_a_single_gift_and_a_single_agent_returns_the_correct_string()
        {
            const string DataFileName = "Converting_a_tracker_to_a_string_with_a_single_gift_and_a_single_agent_returns_the_correct_string";
            var agentWorkTracker = CreateAgentWorkTracker(DataFileName);
            agentWorkTracker.IncrementAgentWorkCount(GiftName, 1L);
            var expected = $@"{GiftName}^{1L.ToString(CultureInfo.CurrentCulture)};1";

            var actual = agentWorkTracker.ToString();

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Converting_a_tracker_to_a_string_with_multiple_gifts_and_agents_contains_all_of_the_data_in_the_tracker()
        {
            // Arrange
            const string DataFileName = "Converting_a_tracker_to_a_string_with_multiple_gifts_and_agents_contains_all_of_the_data_in_the_tracker";
            const string SecondGiftName = "Second";
            const long SecondAgentId = 1L + 1;
            var agentWorkTracker = CreateAgentWorkTracker(DataFileName);

            // First gift first agent
            agentWorkTracker.IncrementAgentWorkCount(GiftName, 1L);

            // First gift second agent
            agentWorkTracker.IncrementAgentWorkCount(GiftName, SecondAgentId);

            // Second gift second agent
            agentWorkTracker.IncrementAgentWorkCount(SecondGiftName, SecondAgentId, 2f);
            var expected = string.Format(CultureInfo.CurrentCulture, "{0}^{1};1^{2};1|{3}^{2};2", GiftName, 1L.ToString(CultureInfo.CurrentCulture), SecondAgentId.ToString(CultureInfo.CurrentCulture),
                SecondGiftName);

            // Act
            var actual = agentWorkTracker.ToString();

            // Assert
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(GiftName + "^1;1", GiftName, 1L, 1f)]
        [InlineData(GiftName + "^1;1^2;2", GiftName, 2L, 2f)]
        [InlineData(GiftName + "^1;1^2;2|Second^1;3", "Second", 1L, 3f)]
        public void Loading_data_from_a_saved_tracker_file_populates_a_valid_tracker([NotNull] string trackerData, [NotNull] string giftName, long agentId, float numberOfTimes)
        {
            // Arrange
            var dataFileName = $"Loading_data_with_a_saved_tracker_file_populates_a_valid_tracker_{giftName}_{agentId}_{numberOfTimes}";
            var agentWorkTracker = CreateAgentWorkTracker(dataFileName, trackerData);

            // Act
            var result = agentWorkTracker.GetLastAgentWorkCountByGift(giftName);

            // Assert
            result.Should().Be(numberOfTimes);
        }

        [Theory]
        [InlineData(GiftName + "^1;1", GiftName, 1L, 1f)]
        [InlineData(GiftName + "^1;1^2;2", GiftName, 2L, 2f)]
        [InlineData(GiftName + "^1;1^2;2|Second^1;3", "Second", 1L, 3f)]
        public void Loading_data_multiple_times_from_a_saved_tracker_file_does_not_duplicate_work_progress([NotNull] string trackerData, [NotNull] string giftName, long agentId, float numberOfTimes)
        {
            var dataFileName = $"Loading_data_with_a_saved_tracker_file_populates_a_valid_tracker_{giftName}_{agentId}_{numberOfTimes}";
            var agentWorkTracker = CreateAgentWorkTracker(dataFileName, trackerData);

            agentWorkTracker.Load();
            agentWorkTracker.Load();

            Assert.Equal(numberOfTimes, agentWorkTracker.GetLastAgentWorkCountByGift(giftName));
        }

        #region Helper Methods

        /// <summary>
        ///     Populates the Harmony Patch with an agent work tracker pointed to our specified test data file.
        /// </summary>
        [NotNull]
        private IAgentWorkTracker CreateAgentWorkTracker(string dataFileName, string trackerData = "", IFileManager fileManager = null)
        {
            fileManager ??= TestExtensions.GetMockFileManager().Object;
            dataFileName = dataFileName.InCurrentDirectory();
            CreateTestTrackerFile(dataFileName, trackerData);

            return new AgentWorkTracker(fileManager, dataFileName);
        }

        private static void CreateTestTrackerFile([NotNull] string fileName, string trackerData)
        {
            var fileNameWithPath = Path.Combine(Directory.GetCurrentDirectory(), fileName);
            File.WriteAllText(fileNameWithPath, trackerData);
        }

        #endregion
    }
}
