using JetBrains.Annotations;
using LobotomyCorporationMods.BadLuckProtectionForGifts;
using LobotomyCorporationMods.BadLuckProtectionForGifts.Interfaces;
using LobotomyCorporationMods.Test.Fakes;
using Xunit;
using Xunit.Extensions;

namespace LobotomyCorporationMods.Test
{
    public sealed class BadLuckProtectionForGiftsTests
    {
        private CreatureEquipmentMakeInfo _creatureEquipmentMakeInfo;
        private UseSkill _useSkill;
        private const long AgentId = 1;
        private const string GiftName = "Test";
        private static IAgentWorkTracker s_agentWorkTracker;

        public BadLuckProtectionForGiftsTests()
        {
            const string DataPath = @"./";
            Harmony_Patch.Initialize(DataPath);
            ClearAgentWorkTracker();
            s_agentWorkTracker = Harmony_Patch.GetAgentWorkTracker();
        }

        /// <summary>
        /// Clears the AgentWorkTracker property by calling the New Game function, which we have
        /// modified to create a new tracker when the player starts a new game. This indirectly tests
        /// that functionality since otherwise almost every test will fail.
        /// </summary>
        private static void ClearAgentWorkTracker()
        {
            Harmony_Patch.CallNewgame(new AlterTitleController());
        }

        [Fact]
        public void ConvertingTrackerToStringContainsDataInTracker()
        {
            const string SecondGiftName = "Second";
            const long SecondAgentId = AgentId + 1;

            // First gift first agent
            s_agentWorkTracker.IncrementAgentWorkCount(GiftName, AgentId);

            // First gift second agent
            s_agentWorkTracker.IncrementAgentWorkCount(GiftName, SecondAgentId);

            // Second gift second agent
            s_agentWorkTracker.IncrementAgentWorkCount(SecondGiftName, SecondAgentId, 2f);
            var expected =
                $@"{GiftName}^{AgentId.ToString()};1^{SecondAgentId.ToString()};1|{SecondGiftName}^{SecondAgentId.ToString()};2";
            var actual = s_agentWorkTracker.ToString();
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ConvertingTrackerToStringWithSingleGiftAndSingleAgentReturnsCorrectString()
        {
            s_agentWorkTracker.IncrementAgentWorkCount(GiftName, AgentId);
            var expected = $@"{GiftName}^{AgentId.ToString()};1";
            var actual = s_agentWorkTracker.ToString();
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("Test^1;1", GiftName, AgentId, 1f)]
        [InlineData("Test^1;1^2;2", GiftName, 2, 2f)]
        [InlineData("Test^1;1^2;2|Second^1;3", "Second", 1, 3f)]
        public void LoadingDataFromSavedTrackerPopulatesAValidAgentWorkTracker([NotNull] string trackerData,
            [NotNull] string giftName, long agentId, float numberOfTimes)
        {
            var agentWorkTracker = s_agentWorkTracker.FromString(trackerData);
            Assert.Equal(numberOfTimes, agentWorkTracker.GetLastAgentWorkCountByGift(giftName));
        }

        [Fact]
        public void ProbabilityBonusDoesNotCauseProbabilityToGoOverOneHundredPercent()
        {
            _creatureEquipmentMakeInfo = new FakeCreatureEquipmentMakeInfo(GiftName);

            // 101 times worked would equal 101% bonus normally
            s_agentWorkTracker.IncrementAgentWorkCount(GiftName, AgentId, 101f);

            // We should only get back 100% even with the 101% bonus
            const float Expected = 1f;
            var actual = 0f;
            Harmony_Patch.GetProb(_creatureEquipmentMakeInfo, ref actual);
            Assert.Equal(Expected, actual);
        }

        [Theory]
        [InlineData(1f)]
        [InlineData(2f)]
        public void ProbabilityIncreasesByOnePercentForEveryTimeAgentWorkedOnCreature(float numberOfTimes)
        {
            s_agentWorkTracker.IncrementAgentWorkCount(GiftName, AgentId, numberOfTimes);
            _creatureEquipmentMakeInfo = new FakeCreatureEquipmentMakeInfo(GiftName);
            var expected = numberOfTimes / 100f;
            var actual = 0f;
            Harmony_Patch.GetProb(_creatureEquipmentMakeInfo, ref actual);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(0f)]
        [InlineData(1f)]
        [InlineData(10f)]
        public void StartingANewGameResetsAgentWorkProgress(float numberOfTimes)
        {
            // Arrange
            s_agentWorkTracker.IncrementAgentWorkCount(GiftName, AgentId);
            var expected = s_agentWorkTracker.GetLastAgentWorkCountByGift(GiftName);
            s_agentWorkTracker.IncrementAgentWorkCount(GiftName, AgentId, numberOfTimes);

            // Act
            Harmony_Patch.CallNewgame(new AlterTitleController());
            s_agentWorkTracker = Harmony_Patch.GetAgentWorkTracker();
            s_agentWorkTracker.IncrementAgentWorkCount(GiftName, AgentId);
            var actual = s_agentWorkTracker.GetLastAgentWorkCountByGift(GiftName);

            // Assert
            Assert.Equal(expected, actual);
        }


        [Fact]
        public void WorkingOnCreatureIncreasesNumberOfTimesWorkedForThatAgent()
        {
            // Arrange
            _useSkill = new FakeUseSkill(GiftName, AgentId);
            s_agentWorkTracker.IncrementAgentWorkCount(GiftName, AgentId);
            var expected = s_agentWorkTracker.GetLastAgentWorkCountByGift(GiftName) + 1;

            // Act
            Harmony_Patch.FinishWorkSuccessfully(_useSkill);
            var actual = s_agentWorkTracker.GetLastAgentWorkCountByGift(GiftName);

            // Assert
            Assert.Equal(expected, actual);
        }
    }
}
