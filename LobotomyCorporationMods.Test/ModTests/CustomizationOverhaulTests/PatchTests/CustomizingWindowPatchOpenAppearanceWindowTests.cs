// SPDX-License-Identifier: MIT

#region

using Customizing;
using FluentAssertions;
using LobotomyCorporationMods.CustomizationOverhaul.Patches;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Test.ModTests.CustomizationOverhaulTests.PatchTests
{
    public sealed class CustomizingWindowPatchOpenAppearanceWindowTests : CustomizationOverhaulModTests
    {
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Opening_the_customize_appearance_window_does_not_increase_the_cost_of_hiring_the_agent(bool isCustomAppearance)
        {
            var sut = InitializeCustomizingWindow();
            sut.CurrentData = new AgentData
            {
                isCustomAppearance = isCustomAppearance,
            };

            sut.PatchAfterOpenAppearanceWindow();

            sut.CurrentData.isCustomAppearance.Should().BeFalse();
        }
    }
}
