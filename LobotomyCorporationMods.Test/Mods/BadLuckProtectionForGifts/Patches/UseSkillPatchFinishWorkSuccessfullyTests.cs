// SPDX-License-Identifier: MIT

#region

using LobotomyCorporationMods.BadLuckProtectionForGifts.Patches;
using LobotomyCorporationMods.Test.Extensions;
using Moq;
using Xunit;
using Xunit.Extensions;

#endregion

namespace LobotomyCorporationMods.Test.Mods.BadLuckProtectionForGifts.Patches
{
    public sealed class UseSkillPatchFinishWorkSuccessfullyTests : BadLuckProtectionForGiftsTests
    {
        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(10)]
        public void Working_on_an_abnormality_increases_the_number_of_successes_for_that_agent(int numberOfSuccesses)
        {
            var mockAgentWorkTracker = CreateMockAgentWorkTracker();
            var useSkill = TestExtensions.CreateUseSkill();
            var creatureEquipmentMakeInfo = GetCreatureEquipmentMakeInfo(GiftName);
            useSkill.targetCreature.metaInfo.equipMakeInfos.Add(creatureEquipmentMakeInfo);
            useSkill.successCount = numberOfSuccesses;

            UseSkillPatchFinishWorkSuccessfully.Prefix(useSkill);

            mockAgentWorkTracker.Verify(tracker => tracker.IncrementAgentWorkCount(GiftName, It.IsAny<long>(), numberOfSuccesses), Times.Once);
        }

        [Fact]
        public void Working_on_an_abnormality_with_no_gift_does_not_increase_the_number_of_successes()
        {
            var mockAgentWorkTracker = CreateMockAgentWorkTracker();
            var useSkill = TestExtensions.CreateUseSkill();

            UseSkillPatchFinishWorkSuccessfully.Prefix(useSkill);

            mockAgentWorkTracker.Verify(static tracker => tracker.IncrementAgentWorkCount(GiftName, It.IsAny<long>(), It.IsAny<int>()), Times.Never);
        }
    }
}
