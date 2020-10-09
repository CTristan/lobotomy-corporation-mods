using LobotomyCorporationMods.BadLuckProtectionForGifts;
using LobotomyCorporationMods.Test.Fakes;
using Xunit;

namespace LobotomyCorporationMods.Test
{
    public sealed class BadLuckProtectionForGiftsTests
    {
        private const long AgentId = 1;
        private const string GiftName = "Test";
        private CreatureEquipmentMakeInfo _creatureEquipmentMakeInfo;
        private UseSkill _useSkill;

        [Fact]
        public void ProbabilityBonusDoesNotCauseProbabilityToGoOverOneHundredPercent()
        {
            _creatureEquipmentMakeInfo = new FakeCreatureEquipmentMakeInfo(GiftName);
            Harmony_Patch.AgentWorkTracker = new AgentWorkTracker();

            // 101 times worked would equal 101% bonus normally
            Harmony_Patch.AgentWorkTracker.IncrementAgentWorkCount(GiftName, AgentId, 101f);

            // We should only get back 100% even with the 101% bonus
            const float expected = 1f;
            var actual = 0f;
            Harmony_Patch.GetProb(_creatureEquipmentMakeInfo, ref actual);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void StartingANewGameResetsAgentWorkProgress()
        {
            Harmony_Patch.File = new FakeFile();
            Harmony_Patch.AgentWorkTracker = new AgentWorkTracker();
            var expected = Harmony_Patch.AgentWorkTracker.GetAgentWorkCount(GiftName, AgentId);
            Harmony_Patch.AgentWorkTracker.IncrementAgentWorkCount(GiftName, AgentId);
            Harmony_Patch.CallNewGame(new AlterTitleController());
            var actual = Harmony_Patch.AgentWorkTracker.GetAgentWorkCount(GiftName, AgentId);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(1f)]
        [InlineData(2f)]
        public void ProbabilityIncreasesByOnePercentForEveryTimeAgentWorkedOnCreature(float numberOfTimes)
        {
            Harmony_Patch.File = new FakeFile();
            Harmony_Patch.AgentWorkTracker = new AgentWorkTracker();
            Harmony_Patch.AgentWorkTracker.IncrementAgentWorkCount(GiftName, AgentId, numberOfTimes);
            _creatureEquipmentMakeInfo = new FakeCreatureEquipmentMakeInfo(GiftName);
            var expected = numberOfTimes / 100f;
            var actual = 0f;
            Harmony_Patch.GetProb(_creatureEquipmentMakeInfo, ref actual);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void WorkingOnCreatureIncreasesNumberOfTimesWorkedForThatAgent()
        {
            _useSkill = new FakeUseSkill(GiftName, AgentId);
            Harmony_Patch.File = new FakeFile();
            Harmony_Patch.AgentWorkTracker = new AgentWorkTracker();
            var expected = Harmony_Patch.AgentWorkTracker.GetAgentWorkCount(GiftName, AgentId) + 1;
            Harmony_Patch.FinishWorkSuccessfully(_useSkill);
            var actual = Harmony_Patch.AgentWorkTracker.GetAgentWorkCount(GiftName, AgentId);
            Assert.Equal(expected, actual);
        }
    }
}
