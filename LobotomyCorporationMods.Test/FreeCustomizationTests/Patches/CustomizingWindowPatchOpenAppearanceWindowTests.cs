// SPDX-License-Identifier: MIT

#region

using Customizing;
using FluentAssertions;
using LobotomyCorporationMods.FreeCustomization.Patches;
using Xunit.Extensions;

#endregion

namespace LobotomyCorporationMods.Test.FreeCustomizationTests.Patches
{
    public sealed class CustomizingWindowPatchOpenAppearanceWindowTests : FreeCustomizationTests
    {
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Opening_the_customize_appearance_window_does_not_increase_the_cost_of_hiring_the_agent(bool isCustomAppearance)
        {
            var customizingWindow = GetCustomizingWindow();
            customizingWindow.CurrentData = new AgentData { isCustomAppearance = isCustomAppearance };

            CustomizingWindowPatchOpenAppearanceWindow.Postfix(customizingWindow);

            customizingWindow.CurrentData.isCustomAppearance.Should().BeFalse();
        }
    }
}
