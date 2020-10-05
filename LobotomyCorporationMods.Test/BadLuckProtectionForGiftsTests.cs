using LobotomyCorporationMods.BadLuckProtectionForGifts;
using Xunit;

namespace LobotomyCorporationMods.Test
{
    public class BadLuckProtectionForGiftsTests
    {
        private readonly CreatureEquipmentMakeInfo _creatureEquipmentMakeInfo;

        public BadLuckProtectionForGiftsTests()
        {
            _creatureEquipmentMakeInfo = new CreatureEquipmentMakeInfo();
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
