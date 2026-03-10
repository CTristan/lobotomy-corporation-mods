// SPDX-License-Identifier: MIT

#region

using AwesomeAssertions;
using Customizing;
using LobotomyCorporationMods.FreeCustomization.Patches;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Test.ModTests.FreeCustomizationTests.PatchTests
{
    public sealed class CustomizingWindowPatchOpenAppearanceWindowTests : FreeCustomizationModTests
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

            _ = sut.CurrentData.isCustomAppearance.Should().BeFalse();
        }
    }
}
