// SPDX-License-Identifier: MIT

#region

using Customizing;
using FluentAssertions;
using LobotomyCorporationMods.FreeCustomization.Patches;
using Xunit.Extensions;

#endregion

namespace LobotomyCorporationMods.Test.Mods.FreeCustomization.Patches
{
    public sealed class CustomizingWindowPatchOpenAppearanceWindowTests : FreeCustomizationTests
    {
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Opening_the_customize_appearance_window_does_not_increase_the_cost_of_hiring_the_agent(bool isCustomAppearance)
        {
            var sut = GetCustomizingWindow();
            sut.CurrentData = new AgentData { isCustomAppearance = isCustomAppearance };

            sut.PatchAfterOpenAppearanceWindow();

            sut.CurrentData.isCustomAppearance.Should().BeFalse();
        }
    }
}
