// SPDX-License-Identifier: MIT

#region

using FluentAssertions;
using LobotomyCorporationMods.BugFixes.Patches;
using LobotomyCorporationMods.Common.Enums;
using LobotomyCorporationMods.Test.Extensions;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Test.Mods.BugFixes.Patches
{
    public sealed class ArmorCreaturePatchOnNoticeTests : BugFixesTests
    {
        [Fact]
        public void Performing_attachment_work_after_replacing_gift_does_not_kill_agent()
        {
            // Arrange
            var notice = NoticeName.OnWorkStart;
            var skill = UnityTestExtensions.CreateUseSkill();
            skill.skillTypeInfo.id = SkillTypeInfo.Consensus;
            var param = new object[] { skill.targetCreature };

            // Act
            var result = ArmorCreaturePatchOnNotice.PatchBeforeOnNotice(notice, param);

            // Assert
            result.Should().BeFalse();
        }

        [Theory]
        [InlineData((int)EquipmentId.CrumblingArmorGift1)]
        [InlineData((int)EquipmentId.CrumblingArmorGift2)]
        [InlineData((int)EquipmentId.CrumblingArmorGift3)]
        [InlineData((int)EquipmentId.CrumblingArmorGift4)]
        public void Performing_attachment_work_with_crumbling_armor_gift_kills_agent(int giftId)
        {
            // Arrange
            var notice = NoticeName.OnWorkStart;
            var param = SetupCrumblingArmorGifts(giftId, SkillTypeInfo.Consensus);

            // Act
            var result = ArmorCreaturePatchOnNotice.PatchBeforeOnNotice(notice, param);

            // Assert
            result.Should().BeTrue();
        }

        [Theory]
        [InlineData((int)EquipmentId.CrumblingArmorGift1, SkillTypeInfo.Amusements)]
        [InlineData((int)EquipmentId.CrumblingArmorGift1, SkillTypeInfo.Cleanliness)]
        [InlineData((int)EquipmentId.CrumblingArmorGift1, SkillTypeInfo.Nutrition)]
        [InlineData((int)EquipmentId.CrumblingArmorGift1, SkillTypeInfo.Violence)]
        [InlineData((int)EquipmentId.CrumblingArmorGift2, SkillTypeInfo.Amusements)]
        [InlineData((int)EquipmentId.CrumblingArmorGift2, SkillTypeInfo.Cleanliness)]
        [InlineData((int)EquipmentId.CrumblingArmorGift2, SkillTypeInfo.Nutrition)]
        [InlineData((int)EquipmentId.CrumblingArmorGift2, SkillTypeInfo.Violence)]
        [InlineData((int)EquipmentId.CrumblingArmorGift3, SkillTypeInfo.Amusements)]
        [InlineData((int)EquipmentId.CrumblingArmorGift3, SkillTypeInfo.Cleanliness)]
        [InlineData((int)EquipmentId.CrumblingArmorGift3, SkillTypeInfo.Nutrition)]
        [InlineData((int)EquipmentId.CrumblingArmorGift3, SkillTypeInfo.Violence)]
        [InlineData((int)EquipmentId.CrumblingArmorGift4, SkillTypeInfo.Amusements)]
        [InlineData((int)EquipmentId.CrumblingArmorGift4, SkillTypeInfo.Cleanliness)]
        [InlineData((int)EquipmentId.CrumblingArmorGift4, SkillTypeInfo.Nutrition)]
        [InlineData((int)EquipmentId.CrumblingArmorGift4, SkillTypeInfo.Violence)]
        public void Performing_non_attachment_work_with_crumbling_armor_gift_will_not_kill_agent(int giftId, long workTypeId)
        {
            // Arrange
            var notice = NoticeName.OnWorkStart;
            var param = SetupCrumblingArmorGifts(giftId, workTypeId);

            // Act
            var result = ArmorCreaturePatchOnNotice.PatchBeforeOnNotice(notice, param);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void Skip_if_not_starting_work_on_an_abnormality()
        {
            var notice = NoticeName.Update;

            var result = ArmorCreaturePatchOnNotice.PatchBeforeOnNotice(notice);

            result.Should().BeTrue();
        }

        [Fact]
        public void Skip_if_agent_will_work_on_tool()
        {
            // Arrange
            var notice = NoticeName.OnWorkStart;
            var param = new object[] { UnityTestExtensions.CreateUnitModel() };

            // Act
            var result = ArmorCreaturePatchOnNotice.PatchBeforeOnNotice(notice, param);

            // Assert
            result.Should().BeTrue();
        }

        #region Helper Methods

        private static object[] SetupCrumblingArmorGifts(int giftId, long skillTypeId)
        {
            var skill = UnityTestExtensions.CreateUseSkill();
            var gift = UnityTestExtensions.CreateEgoGiftModel();
            gift.metaInfo.id = giftId;
            var equipment = UnityTestExtensions.CreateUnitEquipSpace();
            equipment.gifts.addedGifts.Add(gift);
            skill.agent = UnityTestExtensions.CreateAgentModel(equipment: equipment);
            skill.skillTypeInfo.id = skillTypeId;
            var param = new object[] { skill.targetCreature };

            return param;
        }

        #endregion
    }
}
