// SPDX-License-Identifier: MIT

using System;
using Customizing;
using FluentAssertions;
using LobotomyCorporationMods.BugFixes;
using LobotomyCorporationMods.BugFixes.Extensions;
using LobotomyCorporationMods.BugFixes.Patches;
using LobotomyCorporationMods.Common.Enums;
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

        [Theory]
        [InlineData(1, 30, 37)]
        [InlineData(2, 45, 55)]
        [InlineData(3, 65, 75)]
        [InlineData(4, 85, 92)]
        [InlineData(5, 110, 130)]
        public void When_upgrading_an_agent_we_should_always_use_the_original_stat_value_rather_than_the_stat_value_modified_by_bonuses(int currentStatLevel, int minNextStatValue,
            int maxNextStatValue)
        {
            const int BonusStatLevelIncrease = 1;
            var priorStatValue = minNextStatValue - 1;
            var customizingWindow = TestData.DefaultCustomizingWindow;

            customizingWindow.UpgradeStat(priorStatValue, currentStatLevel, BonusStatLevelIncrease, out var result);

            result.Should().BeGreaterOrEqualTo(minNextStatValue).And.BeLessOrEqualTo(maxNextStatValue);
        }

        #endregion

        #region BugFixCrumblingArmor

        [Fact]
        public void Performing_attachment_work_after_replacing_gift_does_not_kill_agent()
        {
            // Arrange
            var notice = NoticeName.OnWorkStart;
            var skill = TestData.DefaultUseSkill;
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
            var skill = TestData.DefaultUseSkill;
            var gift = TestData.DefaultEgoGiftModel;
            gift.metaInfo.id = giftId;
            var equipment = TestExtensions.CreateUnitEquipSpace();
            equipment.gifts.addedGifts.Add(gift);
            skill.agent = TestExtensions.CreateAgentModel(TestData.DefaultAgentName, equipment, TestData.DefaultAgentId, TestData.DefaultAgentNameString, TestData.DefaultWorkerSprite);
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
        public void Class_CustomizingWindow_Method_SetAgentStatBonus_is_patched_correctly_and_does_not_error()
        {
            var patch = typeof(CustomizingWindowPatchSetAgentStatBonus);
            var originalClass = typeof(CustomizingWindow);
            const string MethodName = "SetAgentStatBonus";

            patch.ValidateHarmonyPatch(originalClass, MethodName);

            var result = CustomizingWindowPatchSetAgentStatBonus.Prefix(TestData.DefaultCustomizingWindow, null, null);
            result.Should().BeTrue();
        }

        #endregion
    }
}
