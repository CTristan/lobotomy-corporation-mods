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

            // We should only get back 100% even with the 101% bonus
            const float expected = 1f;
            var actual = 0f;
            Harmony_Patch.GetProb(_creatureEquipmentMakeInfo, ref actual);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(1f)]
        [InlineData(2f)]
        public void ProbabilityIncreasesByOnePercentForEveryTimeAgentWorkedOnCreature(float numberOfTimes)
        {
            Harmony_Patch.NumberOfTimesWorkedByAgent[AgentId] = numberOfTimes;
            var expected = numberOfTimes / 100f;
            var actual = 0f;
            Harmony_Patch.GetProb(_creatureEquipmentMakeInfo, ref actual);
            Assert.Equal(expected, actual);
        }
    }
}
