// SPDX-License-Identifier: MIT

#region

using FluentAssertions;
using LobotomyCorporationMods.BugFixes.Patches;
using LobotomyCorporationMods.Common.Enums;
using Xunit;
using Xunit.Extensions;

#endregion

namespace LobotomyCorporationMods.Test.BugFixesTests
{
    public sealed class BugFixCrumblingArmorTests
    {
        #region BugFixCrumblingArmor

        [Fact]
        public void Performing_attachment_work_after_replacing_gift_does_not_kill_agent()
        {
            // Arrange
            var notice = NoticeName.OnWorkStart;
            var skill = TestExtensions.CreateUseSkill();
            skill.skillTypeInfo.id = SkillTypeInfo.Consensus;
            var param = new object[] { skill.targetCreature };

            // Act
            var result = ArmorCreaturePatchOnNotice.Prefix(notice, param);

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
            var skill = TestExtensions.CreateUseSkill();
            var gift = TestExtensions.CreateEgoGiftModel();
            gift.metaInfo.id = giftId;
            var equipment = TestExtensions.CreateUnitEquipSpace();
            equipment.gifts.addedGifts.Add(gift);
            skill.agent = TestExtensions.CreateAgentModel(equipment: equipment);
            skill.skillTypeInfo.id = SkillTypeInfo.Consensus;
            var param = new object[] { skill.targetCreature };

            // Act
            var result = ArmorCreaturePatchOnNotice.Prefix(notice, param);

            // Assert
            result.Should().BeTrue();
        }

        #endregion
    }
}
