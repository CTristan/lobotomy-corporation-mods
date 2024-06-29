// SPDX-License-Identifier: MIT

#region

using System.Collections.Generic;
using LobotomyCorporationMods.BugFixes.Patches;
using LobotomyCorporationMods.Common.Interfaces.Adapters;
using LobotomyCorporationMods.Test.Extensions;
using Moq;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Test.Mods.BugFixes.Patches
{
    public sealed class CustomizingWindowPatchSetAgentStatBonusTests : BugFixesTests
    {
        /// <summary>
        ///     The original method uses the current stat level instead of the base stat level, so we just need to check that the base stat level is being used in the call rather than
        ///     the current stat level.
        /// </summary>
        [Fact]
        public void When_upgrading_an_agent_we_should_always_use_the_original_stat_level_rather_than_the_stat_level_modified_by_bonuses()
        {
            // Arrange
            const int BaseStatValue = 1;
            const int BuffStatBonus = 100;
            var customizingWindow = UnityTestExtensions.CreateCustomizingWindow();
            var mockAdapter = new Mock<ICustomizingWindowAdapter>();

            // The base stat level is primary stat + title bonus
            // We'll ignore the title bonus
            var primaryStat = UnityTestExtensions.CreateWorkerPrimaryStat();
            primaryStat.battle = BaseStatValue;
            primaryStat.hp = BaseStatValue;
            primaryStat.mental = BaseStatValue;
            primaryStat.work = BaseStatValue;

            // The current stat level is the base stat level + buffs + equipment/gift bonuses
            // We'll add a generic buff to increase all stats
            var statBonus = UnityTestExtensions.CreateWorkerPrimaryStatBonus();
            statBonus.battle = BuffStatBonus;
            statBonus.hp = BuffStatBonus;
            statBonus.mental = BuffStatBonus;
            statBonus.work = BuffStatBonus;
            var statBuff = UnityTestExtensions.CreateUnitStatBuf(statBonus);
            var statBuffList = new List<UnitStatBuf>
            {
                statBuff,
            };

            var agent = UnityTestExtensions.CreateAgentModel(primaryStat: primaryStat, statBufList: statBuffList);
            var data = UnityTestExtensions.CreateAgentData();


            // Act
            customizingWindow.PatchBeforeSetAgentStatBonus(agent, data, mockAdapter.Object);

            // Assert
            // Even though our current stat level is way above 1 due to our buff, we should still only send as our original un-buffed level.
            const int ExpectedStatLevelSent = 1;
            mockAdapter.Verify(adapter => adapter.SetRandomStatValue(It.IsAny<int>(), ExpectedStatLevelSent, It.IsAny<int>()), Times.Exactly(4));
        }
    }
}
