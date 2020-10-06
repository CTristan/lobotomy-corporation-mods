using System.Collections.Generic;
using LobotomyCorporationMods.BadLuckProtectionForGifts;
using Xunit;

namespace LobotomyCorporationMods.Test
{
    public class BadLuckProtectionForGiftsTests
    {
        private const long AgentId = 1;
        private readonly CreatureEquipmentMakeInfo _creatureEquipmentMakeInfo;

        public BadLuckProtectionForGiftsTests()
        {
            _creatureEquipmentMakeInfo = new CreatureEquipmentMakeInfo();
            Harmony_Patch.NumberOfTimesWorkedByAgent = new Dictionary<long, float> {{AgentId, 0f}};
        }

        [Fact]
        public void ProbabilityBonusDoesNotCauseProbabilityToGoOverOneHundredPercent()
        {
            // 101 times worked would equal 101% bonus normally
            Harmony_Patch.NumberOfTimesWorkedByAgent[AgentId] = 101f;
            _creatureEquipmentMakeInfo.prob = 0.01f;

            // We should only get back 100% even with the 101% bonus
            const float expected = 1f;
            Harmony_Patch.GetProb(_creatureEquipmentMakeInfo, out var actual);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(1f)]
        [InlineData(2f)]
        public void ProbabilityIncreasesByOnePercentForEveryTimeAgentWorkedOnCreature(float numberOfTimes)
        {
            Harmony_Patch.NumberOfTimesWorkedByAgent[AgentId] = numberOfTimes;
            _creatureEquipmentMakeInfo.prob = 0.01f;
            var probabilityBonus = numberOfTimes / 100f;
            var expected = probabilityBonus + 0.01f;
            Harmony_Patch.GetProb(_creatureEquipmentMakeInfo, out var actual);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(0f)]
        [InlineData(0.01f)]
        [InlineData(0.1f)]
        [InlineData(1f)]
        public void ValidProbabilityIsReturnedFirstTimeThatAgentWorksOnCreature(float probability)
        {
            _creatureEquipmentMakeInfo.prob = probability;
            var expected = _creatureEquipmentMakeInfo.prob;
            Harmony_Patch.GetProb(_creatureEquipmentMakeInfo, out var actual);
            Assert.Equal(expected, actual);
        }
    }
}
