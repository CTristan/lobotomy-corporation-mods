// SPDX-License-Identifier: MIT

#region

using LobotomyCorporationMods.BadLuckProtectionForGifts.Interfaces;
using LobotomyCorporationMods.BadLuckProtectionForGifts.Patches;
using LobotomyCorporationMods.Test.Extensions;
using Moq;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Test.ModTests.BadLuckProtectionForGiftsTests.PatchTests
{
    public sealed class UseSkillPatchFinishWorkSuccessfullyTests : BadLuckProtectionForGiftsModTests
    {
        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(10)]
        public void Working_on_an_abnormality_increases_the_number_of_successes_for_that_agent(int numberOfSuccesses)
        {
            Mock<IAgentWorkTracker> mockAgentWorkTracker = new();
            var useSkill = UnityTestExtensions.CreateUseSkill();
            var creatureEquipmentMakeInfo = GetCreatureEquipmentMakeInfo(GiftName);
            useSkill.targetCreature.metaInfo.equipMakeInfos.Add(creatureEquipmentMakeInfo);
            useSkill.successCount = numberOfSuccesses;

            useSkill.PatchAfterFinishWorkSuccessfully(mockAgentWorkTracker.Object);

            mockAgentWorkTracker.Verify(tracker => tracker.IncrementAgentWorkCount(GiftName, It.IsAny<long>(), numberOfSuccesses), Times.Once);
        }

        [Fact]
        public void Working_on_an_abnormality_with_no_gift_does_not_increase_the_number_of_successes()
        {
            Mock<IAgentWorkTracker> mockAgentWorkTracker = new();
            var useSkill = UnityTestExtensions.CreateUseSkill();

            useSkill.PatchAfterFinishWorkSuccessfully(mockAgentWorkTracker.Object);

            mockAgentWorkTracker.Verify(tracker => tracker.IncrementAgentWorkCount(GiftName, It.IsAny<long>(), It.IsAny<int>()), Times.Never);
        }
    }
}
