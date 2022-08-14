// SPDX-License-Identifier: MIT

using System;
using Customizing;
using FluentAssertions;
using LobotomyCorporationMods.FreeCustomization;
using LobotomyCorporationMods.FreeCustomization.Patches;
using NSubstitute;
using Xunit;
using Xunit.Extensions;

namespace LobotomyCorporationMods.Test
{
    public sealed class FreeCustomizationTests
    {
        public FreeCustomizationTests()
        {
            _ = new Harmony_Patch();
            var fileManager = TestExtensions.CreateFileManager();
            Harmony_Patch.Instance.LoadData(fileManager);
        }

        /// <summary>
        ///     Harmony requires the constructor to be public.
        /// </summary>
        [Fact]
        public void Constructor_is_public_and_externally_accessible()
        {
            Action act = () => _ = new Harmony_Patch();
            act.ShouldNotThrow();
        }

        [Fact]
        public void The_Appearance_UI_does_not_close_itself_if_there_is_no_close_action()
        {
            var appearanceUi = Substitute.For<AppearanceUI>();

            var result = AppearanceUIPatchCloseWindow.Prefix(appearanceUi);

            result.Should().BeFalse();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Opening_the_customize_appearance_window_does_not_increase_the_cost_of_hiring_the_agent(bool isCustomAppearance)
        {
            var customizingWindow = TestExtensions.CreateCustomizingWindow();
            customizingWindow.CurrentData = new AgentData { isCustomAppearance = true };

            CustomizingWindowPatchOpenAppearanceWindow.Postfix(customizingWindow);

            customizingWindow.CurrentData.isCustomAppearance.Should().BeFalse();
        }

        /// <summary>
        ///     The AgentInfoWindowPatchGenerateWindow patch is untestable because the OpenAppearanceWindow method calls a method
        ///     in another window, and the original method is static which means that we are not able to get an instance to work
        ///     with.
        /// </summary>
        [Fact]
        public void AgentInfoWindowPatchGenerateWindow_IsUntestable()
        {
            var agentInfoWindow = Substitute.For<AgentInfoWindow>();
            var customizingWindow = Substitute.For<CustomizingWindow>();
            customizingWindow.appearanceBlock = TestExtensions.CreateGameObject();
            agentInfoWindow.customizingWindow = customizingWindow;
            AgentInfoWindow.currentWindow = agentInfoWindow;

            var exception = Record.Exception(AgentInfoWindowPatchGenerateWindow.Postfix);

            TestExtensions.AssertIsUnityException(exception).Should().BeTrue();
        }
    }
}
