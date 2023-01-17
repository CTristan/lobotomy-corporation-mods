// SPDX-License-Identifier: MIT

using System;
using Customizing;
using FluentAssertions;
using JetBrains.Annotations;
using LobotomyCorporationMods.BugFixes;
using LobotomyCorporationMods.BugFixes.Extensions;
using LobotomyCorporationMods.BugFixes.Patches;
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
            var customizingWindow = GetDefaultCustomizingWindow();

            customizingWindow.UpgradeStat(priorStatValue, currentStatLevel, BonusStatLevelIncrease, out var result);

            result.Should().BeGreaterOrEqualTo(minNextStatValue).And.BeLessOrEqualTo(maxNextStatValue);
        }

        #endregion

        #region Helper Methods

        [NotNull]
        private static CustomizingWindow GetDefaultCustomizingWindow()
        {
            return TestExtensions.CreateCustomizingWindow();
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
        public void Class_CustomizingWindow_Method_SetAgentStatBonus_is_patched_correctly_and_does_not_error()
        {
            var patch = typeof(CustomizingWindowPatchSetAgentStatBonus);
            var originalClass = typeof(CustomizingWindow);
            const string MethodName = "SetAgentStatBonus";

            patch.ValidateHarmonyPatch(originalClass, MethodName);

            var result = CustomizingWindowPatchSetAgentStatBonus.Prefix(GetDefaultCustomizingWindow(), null, null);
            result.Should().BeTrue();
        }

        #endregion
    }
}
