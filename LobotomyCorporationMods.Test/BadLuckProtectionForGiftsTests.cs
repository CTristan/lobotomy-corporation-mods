using System.Collections.Generic;
using System.Runtime.Serialization;
using LobotomyCorporationMods.BadLuckProtectionForGifts;
using Xunit;

namespace LobotomyCorporationMods.Test
{
    public sealed class BadLuckProtectionForGiftsTests
    {
        private const long AgentId = 1;
        private readonly CreatureEquipmentMakeInfo _creatureEquipmentMakeInfo;
        private readonly UseSkill _useSkill;

        public BadLuckProtectionForGiftsTests()
        {
            _creatureEquipmentMakeInfo = new CreatureEquipmentMakeInfo();
            _useSkill = new UseSkill
            {
                // Calling the AgentModel constructor throws an exception, so we need to create an instance without
                // calling the constructor.
                agent = (AgentModel)FormatterServices.GetSafeUninitializedObject(typeof(AgentModel))
            };
            _useSkill.agent.instanceId = AgentId;
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

        [Fact]
        public void WorkingOnCreatureIncreasesNumberOfTimesWorkedForThatAgent()
        {
            var expected = Harmony_Patch.NumberOfTimesWorkedByAgent[AgentId] + 1;
            Harmony_Patch.FinishWorkSuccessfully(_useSkill);
            var actual = Harmony_Patch.NumberOfTimesWorkedByAgent[AgentId];
            Assert.Equal(expected, actual);
        }
    }
}
