// SPDX-License-Identifier: MIT

#region

using FluentAssertions;
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
        public void Working_on_an_abnormality_increases_the_number_of_successes_for_that_agent(
            int numberOfSuccesses
        )
        {
            var mockAgentWorkTracker = new Mock<IAgentWorkTracker>();
            var mockConfig = CreateMockConfig();
            var useSkill = UnityTestExtensions.CreateUseSkill();
            var creatureEquipmentMakeInfo = GetCreatureEquipmentMakeInfo(GiftName);
            useSkill.targetCreature.metaInfo.equipMakeInfos.Add(creatureEquipmentMakeInfo);
            useSkill.successCount = numberOfSuccesses;

            useSkill.PatchAfterFinishWorkSuccessfully(
                mockAgentWorkTracker.Object,
                mockConfig.Object
            );

            mockAgentWorkTracker.Verify(
                tracker =>
                    tracker.IncrementAgentWorkCount(GiftName, It.IsAny<long>(), numberOfSuccesses),
                Times.Once
            );
        }

        [Fact]
        public void Working_on_an_abnormality_with_no_gift_does_not_increase_the_number_of_successes()
        {
            var mockAgentWorkTracker = new Mock<IAgentWorkTracker>();
            var mockConfig = CreateMockConfig();
            var useSkill = UnityTestExtensions.CreateUseSkill();

            useSkill.PatchAfterFinishWorkSuccessfully(
                mockAgentWorkTracker.Object,
                mockConfig.Object
            );

            mockAgentWorkTracker.Verify(
                tracker =>
                    tracker.IncrementAgentWorkCount(GiftName, It.IsAny<long>(), It.IsAny<int>()),
                Times.Never
            );
        }

        [Fact]
        public void Working_on_an_abnormality_stores_the_risk_level()
        {
            var mockAgentWorkTracker = new Mock<IAgentWorkTracker>();
            var mockConfig = CreateMockConfig();
            var useSkill = UnityTestExtensions.CreateUseSkill();
            var creatureEquipmentMakeInfo = GetCreatureEquipmentMakeInfo(GiftName);
            useSkill.targetCreature.metaInfo.equipMakeInfos.Add(creatureEquipmentMakeInfo);

            useSkill.PatchAfterFinishWorkSuccessfully(
                mockAgentWorkTracker.Object,
                mockConfig.Object
            );

            mockAgentWorkTracker.Verify(
                tracker => tracker.SetRiskLevelForGift(GiftName, It.IsAny<RiskLevel>()),
                Times.Once
            );
        }

        [Fact]
        public void Work_count_resets_when_config_enabled_and_agent_has_gift()
        {
            var mockAgentWorkTracker = new Mock<IAgentWorkTracker>();
            var mockConfig = CreateMockConfig(resetOnGiftReceived: true);
            var useSkill = UnityTestExtensions.CreateUseSkill();
            var creatureEquipmentMakeInfo = GetCreatureEquipmentMakeInfo(GiftName);
            useSkill.targetCreature.metaInfo.equipMakeInfos.Add(creatureEquipmentMakeInfo);

            // Give the agent the gift by adding to their equipment list
            var giftModel = UnityTestExtensions.CreateEgoGiftModel(
                creatureEquipmentMakeInfo.equipTypeInfo
            );
            useSkill.agent.Equipment.gifts.addedGifts.Add(giftModel);

            useSkill.PatchAfterFinishWorkSuccessfully(
                mockAgentWorkTracker.Object,
                mockConfig.Object
            );

            mockAgentWorkTracker.Verify(
                tracker => tracker.ResetAgentWorkCountForGift(GiftName, It.IsAny<long>()),
                Times.Once
            );
        }

        [Fact]
        public void Work_count_does_not_reset_when_config_disabled()
        {
            var mockAgentWorkTracker = new Mock<IAgentWorkTracker>();
            var mockConfig = CreateMockConfig(resetOnGiftReceived: false);
            var useSkill = UnityTestExtensions.CreateUseSkill();
            var creatureEquipmentMakeInfo = GetCreatureEquipmentMakeInfo(GiftName);
            useSkill.targetCreature.metaInfo.equipMakeInfos.Add(creatureEquipmentMakeInfo);

            // Give the agent the gift
            var giftModel = UnityTestExtensions.CreateEgoGiftModel(
                creatureEquipmentMakeInfo.equipTypeInfo
            );
            useSkill.agent.Equipment.gifts.addedGifts.Add(giftModel);

            useSkill.PatchAfterFinishWorkSuccessfully(
                mockAgentWorkTracker.Object,
                mockConfig.Object
            );

            mockAgentWorkTracker.Verify(
                tracker => tracker.ResetAgentWorkCountForGift(It.IsAny<string>(), It.IsAny<long>()),
                Times.Never
            );
        }

        [Fact]
        public void Work_count_does_not_reset_when_agent_does_not_have_gift()
        {
            var mockAgentWorkTracker = new Mock<IAgentWorkTracker>();
            var mockConfig = CreateMockConfig(resetOnGiftReceived: true);
            var useSkill = UnityTestExtensions.CreateUseSkill();
            var creatureEquipmentMakeInfo = GetCreatureEquipmentMakeInfo(GiftName);
            useSkill.targetCreature.metaInfo.equipMakeInfos.Add(creatureEquipmentMakeInfo);

            // Agent does NOT have the gift
            useSkill.PatchAfterFinishWorkSuccessfully(
                mockAgentWorkTracker.Object,
                mockConfig.Object
            );

            mockAgentWorkTracker.Verify(
                tracker => tracker.ResetAgentWorkCountForGift(It.IsAny<string>(), It.IsAny<long>()),
                Times.Never
            );
        }

        [Fact]
        public void Normalized_mode_increments_by_ratio_of_success_to_max_cubes()
        {
            var mockAgentWorkTracker = new Mock<IAgentWorkTracker>();
            var mockConfig = CreateMockConfig(normalizedBonusEnabled: true);
            var useSkill = UnityTestExtensions.CreateUseSkill();
            var creatureEquipmentMakeInfo = GetCreatureEquipmentMakeInfo(GiftName);
            useSkill.targetCreature.metaInfo.equipMakeInfos.Add(creatureEquipmentMakeInfo);
            useSkill.successCount = 5;
            useSkill.maxCubeCount = 10;

            useSkill.PatchAfterFinishWorkSuccessfully(
                mockAgentWorkTracker.Object,
                mockConfig.Object
            );

            // 5/10 = 0.5
            mockAgentWorkTracker.Verify(
                tracker => tracker.IncrementAgentWorkCount(GiftName, It.IsAny<long>(), 0.5f),
                Times.Once
            );
        }

        [Fact]
        public void Normalized_mode_perfect_work_increments_by_one()
        {
            var mockAgentWorkTracker = new Mock<IAgentWorkTracker>();
            var mockConfig = CreateMockConfig(normalizedBonusEnabled: true);
            var useSkill = UnityTestExtensions.CreateUseSkill();
            var creatureEquipmentMakeInfo = GetCreatureEquipmentMakeInfo(GiftName);
            useSkill.targetCreature.metaInfo.equipMakeInfos.Add(creatureEquipmentMakeInfo);
            useSkill.successCount = 10;
            useSkill.maxCubeCount = 10;

            useSkill.PatchAfterFinishWorkSuccessfully(
                mockAgentWorkTracker.Object,
                mockConfig.Object
            );

            // 10/10 = 1.0
            mockAgentWorkTracker.Verify(
                tracker => tracker.IncrementAgentWorkCount(GiftName, It.IsAny<long>(), 1.0f),
                Times.Once
            );
        }

        [Fact]
        public void Normalized_mode_zero_max_cubes_increments_by_zero()
        {
            var mockAgentWorkTracker = new Mock<IAgentWorkTracker>();
            var mockConfig = CreateMockConfig(normalizedBonusEnabled: true);
            var useSkill = UnityTestExtensions.CreateUseSkill();
            var creatureEquipmentMakeInfo = GetCreatureEquipmentMakeInfo(GiftName);
            useSkill.targetCreature.metaInfo.equipMakeInfos.Add(creatureEquipmentMakeInfo);
            useSkill.successCount = 5;
            useSkill.maxCubeCount = 0;

            useSkill.PatchAfterFinishWorkSuccessfully(
                mockAgentWorkTracker.Object,
                mockConfig.Object
            );

            mockAgentWorkTracker.Verify(
                tracker => tracker.IncrementAgentWorkCount(GiftName, It.IsAny<long>(), 0f),
                Times.Once
            );
        }

        [Fact]
        public void Normalized_mode_disabled_uses_raw_success_count()
        {
            var mockAgentWorkTracker = new Mock<IAgentWorkTracker>();
            var mockConfig = CreateMockConfig(normalizedBonusEnabled: false);
            var useSkill = UnityTestExtensions.CreateUseSkill();
            var creatureEquipmentMakeInfo = GetCreatureEquipmentMakeInfo(GiftName);
            useSkill.targetCreature.metaInfo.equipMakeInfos.Add(creatureEquipmentMakeInfo);
            useSkill.successCount = 7;
            useSkill.maxCubeCount = 10;

            useSkill.PatchAfterFinishWorkSuccessfully(
                mockAgentWorkTracker.Object,
                mockConfig.Object
            );

            // Raw successCount, not normalized
            mockAgentWorkTracker.Verify(
                tracker => tracker.IncrementAgentWorkCount(GiftName, It.IsAny<long>(), 7f),
                Times.Once
            );
        }

        private static Mock<IBadLuckProtectionConfig> CreateMockConfig(
            bool resetOnGiftReceived = false,
            bool normalizedBonusEnabled = false
        )
        {
            var mock = new Mock<IBadLuckProtectionConfig>();
            mock.Setup(c => c.ResetOnGiftReceived).Returns(resetOnGiftReceived);
            mock.Setup(c => c.NormalizedBonusEnabled).Returns(normalizedBonusEnabled);
            mock.Setup(c => c.GetBonusPercentageForRiskLevel(It.IsAny<RiskLevel>())).Returns(1.0f);

            return mock;
        }
    }
}
