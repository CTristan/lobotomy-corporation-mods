// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using Customizing;
using FluentAssertions;
using LobotomyCorporationMods.BugFixes;
using LobotomyCorporationMods.BugFixes.Extensions;
using LobotomyCorporationMods.BugFixes.Patches;
using LobotomyCorporationMods.Common.Enums;
using LobotomyCorporationMods.Common.Interfaces.Adapters;
using Moq;
using Xunit;
using Xunit.Extensions;

namespace LobotomyCorporationMods.Test
{
    public sealed class BugFixesTests
    {
        public BugFixesTests()
        {
            _ = new Harmony_Patch();
        }

        #region BugFixStatUpgrades

        /// <summary>
        ///     The original method uses the current stat level instead of the base stat level, so we just need to check that the
        ///     base stat level is being used in the call rather than the current stat level.
        /// </summary>
        [Fact]
        public void When_upgrading_an_agent_we_should_always_use_the_original_stat_level_rather_than_the_stat_level_modified_by_bonuses()
        {
            // Arrange
            const int BaseStatValue = 1;
            const int BuffStatBonus = 100;
            var customizingWindow = TestExtensions.CreateCustomizingWindow();
            var mockAdapter = new Mock<ICustomizingWindowAdapter>();

            // The base stat level is primary stat + title bonus
            // We'll ignore the title bonus
            var primaryStat = TestExtensions.CreateWorkerPrimaryStat();
            primaryStat.battle = BaseStatValue;
            primaryStat.hp = BaseStatValue;
            primaryStat.mental = BaseStatValue;
            primaryStat.work = BaseStatValue;

            // The current stat level is the base stat level + buffs + equipment/gift bonuses
            // We'll add a generic buff to increase all stats
            var statBonus = TestExtensions.CreateWorkerPrimaryStatBonus();
            statBonus.battle = BuffStatBonus;
            statBonus.hp = BuffStatBonus;
            statBonus.mental = BuffStatBonus;
            statBonus.work = BuffStatBonus;
            var statBuff = TestExtensions.CreateUnitStatBuf(statBonus);
            var statBuffList = new List<UnitStatBuf> { statBuff };

            var agent = TestExtensions.CreateAgentModel(primaryStat: primaryStat, statBufList: statBuffList);
            var data = TestExtensions.CreateAgentData();

            // Act
            customizingWindow.UpgradeAgentStats(agent, data, mockAdapter.Object);

            // Assert
            // Even though our current stat level is way above 1 due to our buff, we should still only send as our original un-buffed level.
            const int ExpectedStatLevelSent = 1;
            mockAdapter.Verify(static adapter => adapter.UpgradeAgentStat(It.IsAny<int>(), ExpectedStatLevelSent, It.IsAny<int>()), Times.Exactly(4));
        }

        #endregion

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

        #region Harmony Tests

        /// <summary>
        ///     Harmony requires the constructor to be public.
        /// </summary>
        [Fact]
        public void Constructor_is_public_and_externally_accessible()
        {
            Action action = () => _ = new Harmony_Patch();
            action.ShouldNotThrow();
        }

        [Fact]
        public void Class_ArmorCreature_Method_OnNotice_is_patched_correctly()
        {
            var patch = typeof(ArmorCreaturePatchOnNotice);
            var originalClass = typeof(ArmorCreature);
            const string MethodName = "OnNotice";

            patch.ValidateHarmonyPatch(originalClass, MethodName);
        }

        [Fact]
        public void Class_CustomizingWindow_Method_SetAgentStatBonus_is_patched_correctly()
        {
            var patch = typeof(CustomizingWindowPatchSetAgentStatBonus);
            var originalClass = typeof(CustomizingWindow);
            const string MethodName = "SetAgentStatBonus";

            patch.ValidateHarmonyPatch(originalClass, MethodName);
        }

        #endregion
    }
}
